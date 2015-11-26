#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Text;

namespace UnityTwine.Editor
{
    public class TweeAssetProcessor: AssetPostprocessor
    {
        static void OnPostprocessAllAssets (
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach(string deleted in deletedAssets)
			{
                if (Path.GetExtension(deleted) == "twee") {
                    //AssetDatabase.DeleteAsset(Path.GetFileNameWithoutExtension(deleted) + ".twee.cs");
                   // Debug.Log(deleted);
                }
            }

            foreach(string imported in importedAssets)
			{
                string ext = Path.GetExtension(imported);
				if (ext == ".twee")
				{

					string tweeSource = File.ReadAllText(imported);

					// Remove carriage returns, messes with regex newlines
					// TODO: stream this
					tweeSource = tweeSource.Replace("\r\n", "\n");

					var outputBuffer = new StringBuilder();
					using (StringWriter writer = new StringWriter(outputBuffer))
					{
						new TweeParser().ParseToStream(Path.GetFileNameWithoutExtension(imported), tweeSource, writer);
					}

					string output = outputBuffer.ToString();

					// Detect syntax errors, 
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
							for (int i = 0; i < results.Errors.Count; i++)
							{
								CompilerError error = results.Errors[i];

								// Ignore missing reference errors, we just want syntax
								if (error.ErrorNumber == "CS0246")
									continue;

								if (errors == null)
									errors = new StringBuilder();

								errors.AppendFormat("Line {0:000} - error {1} - {2}\n", error.Line, error.ErrorNumber, error.ErrorText);
							}

							if (errors != null)
							{
								Debug.LogErrorFormat("The Twee file could not be imported due to syntax errors.\n\n{0}", errors);
								Debug.Log(output);
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
					string csFile = Path.Combine(Path.GetDirectoryName(imported), Path.GetFileNameWithoutExtension(imported) + ".cs");
					try
					{
						File.WriteAllText(csFile, output);
					}
                    catch(Exception ex) {
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
}
#endif