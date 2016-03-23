#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using UnityTwine.Editor.Utils;

namespace UnityTwine.Editor.StoryFormats.Harlowe
{
	public class HarloweParser : TwineFormatParser
	{

		public HarloweParser(TwineImporter importer) : base(importer)
		{
		}

		public override void Init()
		{
			// Run the story file in PhantomJS, inject the bridge script that invokes the Harlowe lexer and deserialize the JSON output
			PhantomOutput<HarlowePassageData[]> output = PhantomJS.Analyze<HarlowePassageData[]>(
				new System.Uri(Application.dataPath + "/../" + Importer.AssetPath).AbsoluteUri,
				new System.Uri(Application.dataPath + "/Plugins/UnityTwine/Editor/StoryFormats/Harlowe/.js/harlowe.bridge.js").AbsolutePath
			);

			this.Importer.Passages.AddRange(output.result);
		}

		public override string PassageToCode(TwinePassageData p)
		{
			if (p is HarlowePassageData == false)
				throw new NotSupportedException("HarloweParser called with incompatible passage data");

			var passage = (HarlowePassageData)p;
			StringBuilder outputBuffer = new StringBuilder();

			RenderLexerTokens(passage.Tokens, outputBuffer);

			// Get final string
			string output = outputBuffer.ToString();
			if (output == null || output.Trim().Length == 0)
				output = "yield break;";

			return output;
		}

		void RenderLexerTokens(LexerToken[] tokens, StringBuilder outputBuffer, int indent = 0, bool collapsed = false)
		{
			Indent(indent, outputBuffer);

			for (int t = 0; t < tokens.Length; t++)
			{
				LexerToken token = tokens[t];

				switch (token.type)
				{
					case "text":
						outputBuffer
							.AppendFormat("yield return new TwineText(@\"{0}\");\n", token.text.Replace("\"", "\"\""));
						break;
					case "br":
						if (!collapsed)
							outputBuffer.Append("yield return new TwineText(@\"\\n\");\n");
						break;
					case "twineLink":
						outputBuffer
							.AppendFormat(@"yield return new TwineLink(@""{0}"", @""{1}"", @""{2}"", null, null);",
								token.innerText, token.innerText, token.passage
							)
							.AppendLine();
						break;
				}
			}
		}

		static string Indent(int num, StringBuilder buffer = null)
		{
			var tabsTemp = buffer ?? new StringBuilder();
			for (int i = 0; i < Math.Max(0, num); i++)
				tabsTemp.Append('\t');
			string tabs = tabsTemp.ToString();
			return tabs;
		}
	}

	[Serializable]
	public class LexerToken
	{
		public string type;
		public string name;
		public string text;
		public string innerText;
		public string value;
		public string passage;
		public LexerToken[] tokens;
	}

	[Serializable]
	public class HarlowePassageData : TwinePassageData
	{
		public LexerToken[] Tokens;
	}
}
#endif