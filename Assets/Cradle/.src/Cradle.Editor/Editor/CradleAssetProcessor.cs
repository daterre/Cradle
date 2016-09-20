using Microsoft.CSharp;
using Nustache.Core;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Cradle.Editor
{
    public class CradleAssetProcessor: AssetPostprocessor
    {
		static Regex NameSanitizer = new Regex(@"([^a-z0-9_]|^[0-9])", RegexOptions.IgnoreCase);
		static char[] InvalidFileNameChars = System.IO.Path.GetInvalidFileNameChars();
		static Dictionary<string, Type> ImporterTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        static void OnPostprocessAllAssets (
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach(string assetPath in importedAssets)
			{
				// ======================================
				// Choose the importer for this file type

                string ext = Path.GetExtension(assetPath).ToLower();
				if (string.IsNullOrEmpty(ext))
					continue;

				// Get the right importer for this type
				ext = ext.Substring(1);
				StoryImporter importer = null;
				Type importerType;
				if (!ImporterTypes.TryGetValue(ext, out importerType))
					continue;

				importer = (StoryImporter)Activator.CreateInstance(importerType);
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
					Debug.LogErrorFormat("The story cannot be imported because \"{0}\" is not a valid Unity script name.", fileName);
					continue;
				}

				// ======================================
				// Initialize the importer - load data and choose transcoder

				try
				{
					importer.Initialize();
				}
				catch (StoryImportException ex)
				{
					Debug.LogErrorFormat("Story import failed: {0} ({1})", ex.Message, fileNameExt);
					continue;
				}

				// ======================================
				// Run the transcoder

				try
				{
					importer.Transcode();
				}
				catch (StoryFormatTranscodeException ex)
				{
					Debug.LogErrorFormat("Story format transcoding failed at passage {0}: {1} ({2})", ex.Passage, ex.Message, fileNameExt);
					continue;
				}

				// ======================================
				// Generate code

				StoryFormatMetadata storyFormatMetadata = importer.Metadata;
                TemplatePassageData[] passageData = importer.Passages.Select(p => new TemplatePassageData()
                    {
                        Pid = p.Pid,
                        Name = p.Name.Replace("\"", "\"\""),
                        Tags = p.Tags,
                        Code = p.Code.Main.Split(new string[]{ Environment.NewLine }, StringSplitOptions.None),
                        Fragments = p.Code.Fragments.Select((frag, i) => new TemplatePassageFragment()
                            {
                                Pid = p.Pid,
                                FragId = i,
                                Code = frag.Split(new string[]{ Environment.NewLine }, StringSplitOptions.None)
                            }).ToArray()
                    }).ToArray();

				// Get template file from this editor script's directory
				string output = Nustache.Core.Render.FileToString(
					Path.Combine(Application.dataPath, "Cradle/Editor/Templates/Story.template"),
					new Dictionary<string, object>()
					{
						{"timestamp", DateTime.Now.ToString("G")},
						{"version", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()},
						{"originalFile", Path.GetFileName(assetPath)},
						{"storyFormatName", storyFormatMetadata.StoryFormatName},
						{"storyFormatNamespace", storyFormatMetadata.StoryBaseType.Namespace},
						{"storyFormatClass", storyFormatMetadata.StoryBaseType.FullName},
						{"storyName", storyName},
						{"startPassage", storyFormatMetadata.StartPassage ?? "Start"},
						{"vars", importer.Vars},
						{"macroLibs", importer.MacroLibs},
						{"strictMode", storyFormatMetadata.StrictMode ? "true" : "false"},
                        {"passages", passageData}
					}
				);

				// ======================================
				// Compile

                // Unity bug fix: This environment variable is not set on Mac for some reason - https://github.com/AngryAnt/Behave-release/issues/21
                if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    string monoBinPath = Path.Combine(EditorApplication.applicationContentsPath, "Frameworks/Mono/bin");

                    // Unity 5.4 and up
                    if (!Directory.Exists(monoBinPath))
                        monoBinPath = Path.Combine(EditorApplication.applicationContentsPath, "Mono/bin");

                    // Huh?
                    if (!Directory.Exists(monoBinPath))
                        Debug.LogError("For some reason I can't find the Mono directory inside Unity.app. Please open an issue on github.com/daterre/Cradle");

                    Environment.SetEnvironmentVariable("PATH", string.Format("{0}:{1}",
                            Environment.GetEnvironmentVariable("PATH"),
                            monoBinPath
                        ));
                }

				// Detect syntax errors
				
                var compilerSettings = new CompilerParameters()
                {
                    GenerateInMemory = true,
                    GenerateExecutable = false
                };
                foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    if (!string.IsNullOrEmpty(assembly.Location))
                        compilerSettings.ReferencedAssemblies.Add(assembly.Location);

				var results = new CSharpCodeProvider().CompileAssemblyFromSource(compilerSettings, output);

				if (results.Errors.Count > 0)
				{
					int errorLineOffset = Application.platform == RuntimePlatform.OSXEditor ? 3 : 4;

					bool errors = false;
					for (int i = 0; i < results.Errors.Count; i++)
					{
						CompilerError error = results.Errors[i];

						switch (error.ErrorNumber)
						{
							//case "CS0246":
							case "CS0103":
							case "":
								continue;
								
							// Single quotes instead of double quotes
							case "CS1012":
								error.ErrorText = "Strings must use \"double-quotes\", not 'single-quotes'.";
								break;

							// Double var accessor
							case "CS1612":
								error.ErrorText = "Can't set a nested property directly like that. Use a temporary variable in-between.";
								break;
						}

						// Error only if not a warning
						errors |= !error.IsWarning;

						try
						{
							// Get some compilation metadata - relies on the template using the #frag# token
							string[] errorDirective = error.FileName.Split(new string[] { "#frag#" }, StringSplitOptions.None);
							string errorPassage = errorDirective[0];
							int errorFragment = errorDirective.Length > 1 ? int.Parse(errorDirective[1]) : -1;
							TemplatePassageData passage = passageData.Where(p => p.Name == errorPassage).FirstOrDefault();
							string lineCode = passage == null || error.Line < errorLineOffset ? "(code not available)" : errorFragment >= 0 ?
								passage.Fragments[errorFragment].Code[error.Line - errorLineOffset] :
								passage.Code[error.Line - errorLineOffset];

							if (error.IsWarning)
								Debug.LogWarningFormat("Story compilation warning at passage '{0}': {1}\n\n\t{2}\n",
									errorPassage,
									error.ErrorText,
									lineCode
								);
							else
								Debug.LogErrorFormat("Story compilation error at passage '{0}': {1}\n\n\t{2}\n",
									errorPassage,
									error.ErrorText,
									lineCode
								);
						}
						catch
						{
							if (error.IsWarning)
								Debug.LogWarningFormat("Story compilation warning: {0}\n",
									error.ErrorText
								);
							else
								Debug.LogErrorFormat("Story compilation error: {0}\n",
									error.ErrorText
								);
						}
					}

					if (errors)
					{
						Debug.LogErrorFormat("The story {0} has some errors and can't be imported (see console for details).",
							Path.GetFileName(assetPath)
						);
						continue;
					}
				}

				// Remove custom line directives so they won't interfere with debugging the final script
				output = Regex.Replace(output, @"^\s*\#line\b.*$", string.Empty, RegexOptions.Multiline);

				// Passed syntax check, save to file
				string csFile = Path.Combine(Path.GetDirectoryName(assetPath), Path.GetFileNameWithoutExtension(assetPath) + ".cs");
				File.WriteAllText(csFile, output, Encoding.UTF8);

				// Need to do it twice - on the second time it compiles the script
				// http://answers.unity3d.com/questions/14367/how-can-i-wait-for-unity-to-recompile-during-the-e.html
				AssetDatabase.ImportAsset(csFile, ImportAssetOptions.ForceSynchronousImport);
                AssetDatabase.ImportAsset(csFile, ImportAssetOptions.ForceSynchronousImport);

                // ======================================
                // Organize the assets

                #region Disabled prefab creation because the story class can't be added during this asset import
                /*
                // Find the story class
                string projectDir = Directory.GetParent((Path.GetFullPath(Application.dataPath))).FullName;
                Type storyClass = null;
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    // Skip references external to the project
                    if (!string.IsNullOrEmpty(assembly.Location) && !Path.GetFullPath(assembly.Location).StartsWith(projectDir, StringComparison.OrdinalIgnoreCase))
                        continue;
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type.Name == storyName)
                        {
                            storyClass = type;
                            break;
                        }
                    }

                    if (storyClass != null)
                        break;
                }

                if (storyClass == null)
                {
                    Debug.LogWarning("UnityTwine successfully imported the story, but a prefab couldn't be made for you. Sorry :(");
                    continue;
                }

                // Create a prefab
                var prefab = new GameObject();
                prefab.name = storyName;
                prefab.AddComponent(storyClass);

                PrefabUtility.CreatePrefab(Path.Combine(Path.GetDirectoryName(assetPath), Path.GetFileNameWithoutExtension(assetPath) + ".prefab"), prefab, ReplacePrefabOptions.Default);
                */
                #endregion

            }
        }


		public class TemplatePassageData : PassageData
		{
			public new string[] Code;
			public TemplatePassageFragment[] Fragments;

			public string DirectiveName
			{
				get
				{

					string corrected = String.Join("_", this.Name.Split(InvalidFileNameChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
					return corrected;
				}
			}
		}

		public class TemplatePassageFragment
		{
			public string Pid;
			public int FragId;
			public string[] Code;
		}

		public static void RegisterImporter<T>(string extenstion) where T : StoryImporter, new()
		{
			ImporterTypes[extenstion] = typeof(T);
		}
    }
}