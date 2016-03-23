using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityTwine.Editor.Utils
{
	public static class PhantomJS
	{
		public static PhantomOutput<ResultT> Analyze<ResultT>(string storyFileUri, string bridgeScriptPath, bool throwExOnError = true)
		{
			// Run the HTML in PhantomJS
			var phantomJS = new System.Diagnostics.Process();
			phantomJS.StartInfo.UseShellExecute = false;
			phantomJS.StartInfo.CreateNoWindow = true;
			phantomJS.StartInfo.RedirectStandardOutput = true;
			phantomJS.StartInfo.WorkingDirectory = Application.dataPath + "/Plugins/UnityTwine/Editor/StoryFormats/.js";
			phantomJS.StartInfo.FileName = Application.dataPath + "/Plugins/UnityTwine/Editor/ThirdParty/PhantomJS/bin/phantomjs.exe";
			phantomJS.StartInfo.Arguments = string.Format("\"{0}\" \"{1}\" \"{2}\"",
				"phantom.js",
				storyFileUri,
				bridgeScriptPath
			);
			phantomJS.Start();
			string outputJson = phantomJS.StandardOutput.ReadToEnd();
			phantomJS.WaitForExit();

			PhantomOutput<ResultT> output = JsonUtility.FromJson<PhantomOutput<ResultT>>(outputJson);

			if (throwExOnError)
			{
				StringBuilder errors = null;
				foreach (PhantomConsoleMessage msg in output.console)
				{
					if (msg.type == "message")
						continue;
					errors = errors ?? new StringBuilder("Errors while parsing the story file:\n\n");
					errors.AppendLine(msg.value);
					if (msg.trace != null)
						errors.AppendLine(msg.trace);
				}
				if (errors != null)
					throw new TwineImportException(errors.ToString());
			}

			return output;
		}
	}

	[Serializable]
	public class PhantomOutput<ResultT>
	{
		public PhantomConsoleMessage[] console;
		public ResultT result;
	}

	[Serializable]
	public class PhantomConsoleMessage
	{
		public string type;
		public string value;
		public string trace;
	}
}
