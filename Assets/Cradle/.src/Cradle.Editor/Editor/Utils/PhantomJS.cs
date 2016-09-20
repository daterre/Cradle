using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cradle.Editor.ThirdParty.FullSerializer;

namespace Cradle.Editor.Utils
{
	public static class PhantomJS
	{
		public static PhantomOutput<ResultT> Run<ResultT>(string url, string bridgeScriptPath = null, bool throwExOnError = true)
		{
			string binPath =
				Application.platform == RuntimePlatform.OSXEditor ? "/Cradle/Editor/ThirdParty/PhantomJS/bin/osx/phantomjs" :
				Application.platform == RuntimePlatform.WindowsEditor ? "/Cradle/Editor/ThirdParty/PhantomJS/bin/win/phantomjs.exe" :
				null;

			if (binPath == null)
				throw new NotSupportedException ("Editor platform not supported.");

			// Run the HTML in PhantomJS
			var phantomJS = new System.Diagnostics.Process();
			phantomJS.StartInfo.UseShellExecute = false;
			phantomJS.StartInfo.CreateNoWindow = true;
			phantomJS.StartInfo.RedirectStandardOutput = true;
			phantomJS.StartInfo.WorkingDirectory = Application.dataPath + "/Cradle/Editor/js";
			phantomJS.StartInfo.FileName = Application.dataPath + binPath;
			phantomJS.StartInfo.Arguments = string.Format ("\"{0}\" \"{1}\"{2}",
				"phantom.js_",
				url,
				bridgeScriptPath == null ? null : string.Format(" \"{0}\"",bridgeScriptPath)
            );
           
            // On Mac, the phantomjs binary requires execute permissions (755), this should be set by Install.cs in the Phantom directory
            phantomJS.Start();
			
            string outputJson = phantomJS.StandardOutput.ReadToEnd();
			phantomJS.WaitForExit();

			//PhantomOutput<ResultT> output = JsonUtility.FromJson<PhantomOutput<ResultT>>(outputJson);
			var output = (PhantomOutput<ResultT>) FullSerializerWrapper.Deserialize(typeof(PhantomOutput<ResultT>), outputJson);

			bool hasErrors = false;
			
			foreach (PhantomConsoleMessage msg in output.console)
			{
				if (msg.type != "error")
					continue;
				
				hasErrors = true;
				Debug.LogErrorFormat("{0}\n\n{1}\n\n", msg.value, msg.trace);
			}
			if (hasErrors && throwExOnError)
				throw new StoryImportException("HTML errors detected");

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

	public static class FullSerializerWrapper {
		private static readonly fsSerializer _serializer = new fsSerializer();

		public static string Serialize(Type type, object value) {
			// serialize the data
			fsData data;
			_serializer.TrySerialize(type, value, out data).AssertSuccessWithoutWarnings();

			// emit the data via JSON
			return fsJsonPrinter.CompressedJson(data);
		}

		public static object Deserialize(Type type, string serializedState) {
			// step 1: parse the JSON data
			fsData data = fsJsonParser.Parse(serializedState);

			// step 2: deserialize the data
			object deserialized = null;
			_serializer.TryDeserialize(data, type, ref deserialized).AssertSuccessWithoutWarnings();

			return deserialized;
		}
	}
}
