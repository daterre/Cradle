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

        static void OnPostprocessAllAssets (
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach(string assetPath in importedAssets)
			{
				// ======================================
				// STEP 1: Validate

				TwineImporter importer = null;

                string ext = Path.GetExtension(assetPath).ToLower();

				if (ext == ".twee")
					importer = new Importers.TweeImporter(assetPath);
				else if (ext == ".html")
					importer = new Importers.PublishedHtmlImporter(assetPath);
				else
					return;

				if (!importer.Validate())
					return;

				string fileName = Path.GetFileNameWithoutExtension(assetPath);
				string storyName = NameSanitizer.Replace(fileName, string.Empty);
				if (storyName != fileName)
				{
					Debug.LogErrorFormat("UnityTwine cannot import the story because \"{0}\" is not a valid Unity script name.", fileName);
					continue;
				}

				// ======================================
				// STEP 2: Load and transcode

				try
				{
					importer.Load();
					importer.Transcode();
				}
				catch(TwineTranscodingException ex)
				{
					Debug.LogErrorFormat("Transcoding failed: {0} (passage: {1})", ex.Message, ex.Passage);
					continue;
				}
				catch(Exception ex)
				{
					Debug.LogException(ex);
					continue;
				}

				// ======================================
				// STEP 3: Generate code

				StoryFormatMetadata storyFormatMetadata = importer.Transcoder.Metadata;

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
						{"runtimeMacrosClass", storyFormatMetadata.RuntimeMacrosType.FullName},
						{"vars", importer.Vars},
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
						StringBuilder errors = null;
						string[] lines = null;
						for (int i = 0; i < results.Errors.Count; i++)
						{
							CompilerError error = results.Errors[i];

							// Ignore missing reference errors, we just want syntax
							if (error.ErrorNumber == "CS0246")
								continue;

							if (errors == null)
							{
								errors = new StringBuilder();
								lines = output.Replace("\r", "").Split('\n');
							}

							errors.AppendFormat("Line {0:000} - error {1} - {2}\n\t{3}\n",
								error.Line,
								error.ErrorNumber,
								error.ErrorText,
								error.Line > 0 && error.Line < lines.Length ? lines[error.Line - 1].Trim() : null
							);
						}

						if (errors != null)
						{
							Debug.LogErrorFormat("The Twine story {0} could not be imported due to syntax errors.\n\n{1}",
								Path.GetFileName(assetPath),
								errors
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