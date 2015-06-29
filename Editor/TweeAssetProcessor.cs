#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

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
                    try
					{
						string tweeSource = File.ReadAllText(imported);

						// Remove carriage returns, messes with regex newlines
						// TODO: stream this
						tweeSource = tweeSource.Replace("\r\n", "\n");

						string csFile = Path.Combine(Path.GetDirectoryName(imported), Path.GetFileNameWithoutExtension(imported) + ".cs");

						using (FileStream file = File.Open(csFile, FileMode.Create))
						{
							using (StreamWriter writer = new StreamWriter(file))
							{
								new TweeParser().ParseToStream(Path.GetFileNameWithoutExtension(imported), tweeSource, writer);
							}
						}
						// Need to do it twice - on the second time it compiles the script
						// http://answers.unity3d.com/questions/14367/how-can-i-wait-for-unity-to-recompile-during-the-e.html
						AssetDatabase.ImportAsset(csFile, ImportAssetOptions.ForceSynchronousImport);
						AssetDatabase.ImportAsset(csFile, ImportAssetOptions.ForceSynchronousImport);
                    }
                    catch(Exception ex) {
                        Debug.LogError(ex);
                    }
                }
            }
        }
    }
}
#endif