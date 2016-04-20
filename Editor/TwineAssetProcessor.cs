#if UNITY_EDITOR
using Microsoft.CSharp;
using Nustache.Core;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace UnityTwine.Editor
{
    public class TwineAssetProcessor: AssetPostprocessor
    {
		static Regex NameSanitizer = new Regex(@"([^a-z0-9_]|^[0-9])", RegexOptions.IgnoreCase);
		static Dictionary<string, Type> ImporterTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

		public class TemplatePassageData : TwinePassageData
		{
			public string[] Code;
			public new TemplatePassageFragment[] Fragments;
		}

		public class TemplatePassageFragment
		{
			public string Pid;
			public int FragId;
			public string[] Code;
		}

		public static void RegisterImporter<T>(string extenstion) where T: TwineImporter, new()
		{
			ImporterTypes[extenstion] = typeof(T);
		}

        static void OnPostprocessAllAssets (
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach(string assetPath in importedAssets)
			{
				// ======================================
				// Choose the Twine importer for this file type

                string ext = Path.GetExtension(assetPath).ToLower();
				if (string.IsNullOrEmpty(ext))
					continue;

				// Get the right importer for this type
				ext = ext.Substring(1);
				TwineImporter importer = null;
				Type importerType;
				if (!ImporterTypes.TryGetValue(ext, out importerType))
					continue;

				importer = (TwineImporter)Activator.CreateInstance(importerType);
				importer.AssetPath = assetPath;

				// ======================================
				// Validate the file

				// Check that the file is relevant
				if (!importer.IsAssetRelevant())
					return;

				// Check that the story name is valid
				string fileNameExt = Path.GetFileName(assetPath);
				string fileName = Path.GetFileNameWithoutExtension(assetPath);
				string storyName = NameSanitizer.Replace(fileName, string.Empty);
				if (storyName != fileName)
				{
					Debug.LogErrorFormat("UnityTwine cannot import the story because \"{0}\" is not a valid Unity script name.", fileName);
					continue;
				}

				// ======================================
				// Initialize the importer - load data and choose transcoder

				try
				{
					importer.Initialize();
				}
				catch (TwineImportException ex)
				{
					Debug.LogErrorFormat("Twine import failed: {0} ({1})", fileNameExt, ex.Message);
					continue;
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
					continue;
				}

				// ======================================
				// Run the transcoder

				try
				{
					importer.Transcode();
				}
				catch (TwineTranscodeException ex)
				{
					Debug.LogErrorFormat("Twine transcoding failed at passage {0}: {1} ({2})", ex.Passage, ex.Message, fileNameExt);
					continue;
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
					continue;
				}

				// ======================================
				// Generate code

				StoryFormatMetadata storyFormatMetadata = importer.Metadata;

				// Get template file from this editor script's directory
				string output = Nustache.Core.Render.FileToString(
					Path.Combine(Application.dataPath, "Plugins/UnityTwine/Editor/Templates/TwineStory.template"),
					new Dictionary<string, object>()
					{
						{"timestamp", DateTime.Now.ToString("G")},
						{"version", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()},
						{"originalFile", Path.GetFileName(assetPath)},
						{"storyFormatName", storyFormatMetadata.StoryFormatName},
						{"storyFormatNamespace", storyFormatMetadata.StoryBaseType.Namespace},
						{"storyFormatClass", storyFormatMetadata.StoryBaseType.FullName},
						{"storyName", storyName},
						{"vars", importer.Vars},
						{"macroLibs", importer.MacroLibs},
						{"strictMode", storyFormatMetadata.StrictMode ? "true" : "false"},
						{"passages", importer.Passages.Select(p => new TemplatePassageData(){
								Pid = p.Pid,
								Name = p.Name,
								Tags = p.Tags,
								Code = p.Code.Main.Split(new string[]{Environment.NewLine}, StringSplitOptions.None),
								Fragments = p.Code.Fragments.Select((frag,i) => new TemplatePassageFragment(){
									Pid = p.Pid,
									FragId = i,
									Code = frag.Split(new string[]{Environment.NewLine}, StringSplitOptions.None)
								}).ToArray()
							}).ToArray()
						}
					}
				);

				// ======================================
				// STEP 4: Compile

				#if UNITY_EDITOR_OSX
				Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ":/Applications/Unity/Unity.app/Contents/Frameworks/Mono/bin");
				#endif

				// Detect syntax errors
				try
				{
					var results = new CSharpCodeProvider().CompileAssemblyFromSource(new CompilerParameters()
					{
						GenerateInMemory = true,
						GenerateExecutable = false
					}, output);

					if (results.Errors.Count > 0)
					{
						bool errors = false;
						string[] lines = null;
						for (int i = 0; i < results.Errors.Count; i++)
						{
							CompilerError error = results.Errors[i];

							switch (error.ErrorNumber)
							{
								// Ignore missing reference errors, we just want syntax
								case "CS0246":
									continue;
								
								// Single quotes instead of double quotes
								case "CS1012":
									error.ErrorText = "UnityTwine requires strings to use double-quotes, not single-quotes.";
									break;
							}
							

							if (!errors)
							{
								errors = true;
								lines = output.Replace("\r", "").Split('\n'); // TODO: split by proper line ending
							}

							Debug.LogErrorFormat("{0}\n\n{1}",
								error.ErrorText,
								error.Line > 0 && error.Line < lines.Length ? lines[error.Line - 1].Trim() : null
							);
						}

						if (errors)
						{
							Debug.LogErrorFormat("The Twine story {0} could not be imported due to syntax errors.",
								Path.GetFileName(assetPath)
							);
							//Debug.LogError(output);
							//continue;
						}
					};
				}
				catch (Exception ex)
				{
					Debug.LogError(ex);
					continue;
				}

				// Passed syntax check, save to file
				string csFile = Path.Combine(Path.GetDirectoryName(assetPath), Path.GetFileNameWithoutExtension(assetPath) + ".cs");
				try
				{
					File.WriteAllText(csFile, output, Encoding.UTF8);
				}
				catch (Exception ex)
				{
					Debug.LogError(ex);
					continue;
				}

				// Need to do it twice - on the second time it compiles the script
				// http://answers.unity3d.com/questions/14367/how-can-i-wait-for-unity-to-recompile-during-the-e.html
				AssetDatabase.ImportAsset(csFile, ImportAssetOptions.ForceSynchronousImport);
				AssetDatabase.ImportAsset(csFile, ImportAssetOptions.ForceSynchronousImport);
            }
        }

    }
}
#endif