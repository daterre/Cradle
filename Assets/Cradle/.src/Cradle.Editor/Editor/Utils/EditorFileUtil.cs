using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cradle.Editor.ThirdParty.FullSerializer;

namespace Cradle.Editor.Utils
{
	public static class EditorFileUtil
	{
		public static string FindFile(string fileName, bool directoryOnly = false)
		{
			string[] paths = Directory.GetFiles(Application.dataPath, fileName, SearchOption.AllDirectories);
			if (paths.Length < 1)
				throw new StoryImportException(string.Format("Could not find the file '{0}'. Did you install Cradle correctly?", fileName));
			
			if (paths.Length > 1)
				throw new StoryImportException(string.Format("Found more than one file called '{0}'. Did you install Cradle correctly?", fileName));

			string file = paths[0];
			return directoryOnly ? Path.GetDirectoryName(file) : file;
		}
	}
}
