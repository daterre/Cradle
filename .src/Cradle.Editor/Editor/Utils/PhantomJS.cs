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
	public static class PhantomJS
	{
		public static PhantomOutput<ResultT> Run<ResultT>(string url, string bridgeScriptFileName = null, bool throwExOnError = true)
		{
			// Get the location of the phantom exe
			string phantomExecutable =
				Application.platform == RuntimePlatform.OSXEditor ? "phantomjs" :
				Application.platform == RuntimePlatform.WindowsEditor ? "phantomjs.exe" :
				null;

			if (phantomExecutable == null)
				throw new NotSupportedException("Editor platform not supported.");

			string binPath = EditorFileUtil.FindFile(phantomExecutable);

			// Get the location of the phantom script
			const string phantomJs = "phantom.js_";
			string jsPath = EditorFileUtil.FindFile(phantomJs, true);

			// Get the location of the bridge script
			string bridgePath = bridgeScriptFileName != null ? EditorFileUtil.FindFile(bridgeScriptFileName) : null;

			// Run the HTML in PhantomJS
			var phantomJS = new System.Diagnostics.Process();
			phantomJS.StartInfo.UseShellExecute = false;
			phantomJS.StartInfo.CreateNoWindow = true;
			phantomJS.StartInfo.RedirectStandardOutput = true;
			phantomJS.StartInfo.WorkingDirectory = jsPath;
			phantomJS.StartInfo.FileName = binPath;
			phantomJS.StartInfo.Arguments = string.Format ("\"{0}\" \"{1}\"{2}",
				phantomJs,
				url,
				bridgePath == null ? null : string.Format(" \"{0}\"", bridgePath)
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
