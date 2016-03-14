#if UNITY_EDITOR
using Microsoft.CSharp;
using Nustache.Core;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UnityTwine.Editor
{
    public class TwineAssetProcessor: AssetPostprocessor
    {
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
				//else if (ext == ".html")
					//importer = new Importers.HtmlImporter(assetPath);
				else
					return;

				// ======================================
				// STEP 2: Load and parse

				importer.Load();
				importer.Prepare();
				importer.Parse();

				// ======================================
				// STEP 3: Generate code

				// Get template file from this editor script's directory
				string output = Nustache.Core.Render.FileToString(
					Path.Combine(Application.dataPath, "Plugins/UnityTwine/Editor/TwineStory.template"),
					new Dictionary<string, object>()
					{
						{"originalFile", Path.GetFileName(assetPath)},
						{"storyName", Path.GetFileNameWithoutExtension(assetPath)}, // TODO: only alphanumeric characters
						{"timestamp", DateTime.Now.ToString("G")},
						{"vars", importer.Vars.Keys},
						{"passages", importer.Passages}
					}
				);

				// ======================================
				// STEP 4: Compile

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
								lines[error.Line - 1].Trim()
							);
						}

						if (errors != null)
						{
							Debug.LogErrorFormat("The Twine story {0} could not be imported due to syntax errors.\n\n{1}",
								Path.GetFileName(assetPath),
								errors
							);
							//Debug.LogError(output);
							return;
						}
					};
				}
				catch (Exception ex)
				{
					Debug.LogError(ex);
					return;
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
					return;
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