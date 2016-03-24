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
		class Code
		{
			public StringBuilder Buffer = new StringBuilder();
			public int Indent = 0;
			public bool Collapsed = false;
		}

		Code _code;
		HarlowePassageData _input;
		TwinePassageCode _output;

		static Regex rx_LinkNames = new Regex(@"((?'linkName'[^|\n]+?)\s*=\s*)?(?'linkText'.*)",
			RegexOptions.IgnoreCase |
			RegexOptions.ExplicitCapture);

		public HarloweParser(TwineImporter importer) : base(importer)
		{
		}

		public override void Init()
		{
			// Run the story file in PhantomJS, inject the bridge script that invokes the Harlowe lexer and deserialize the JSON output
			PhantomOutput<HarlowePassageData[]> output = PhantomJS.Run<HarlowePassageData[]>(
				new System.Uri(Application.dataPath + "/../" + Importer.AssetPath).AbsoluteUri,
				new System.Uri(Application.dataPath + "/Plugins/UnityTwine/Editor/StoryFormats/Harlowe/.js/harlowe.bridge.js").AbsolutePath
			);

			this.Importer.Passages.AddRange(output.result);
		}

		public override TwinePassageCode PassageToCode(TwinePassageData p)
		{
			if (p is HarlowePassageData == false)
				throw new NotSupportedException("HarloweParser called with incompatible passage data");

			_input = (HarlowePassageData)p;
			_output = new TwinePassageCode();
			_code = new Code();

			GenerateBody(_input.Tokens);

			// Get final string
			string code = _code.Buffer.ToString();
			if (code == null || code.Trim().Length == 0)
				code = "yield break;";
			_output.Main = code;

			return _output;
		}

		void GenerateBody(LexerToken[] tokens)
		{
			for (int t = 0; t < tokens.Length; t++)
			{
				LexerToken token = tokens[t];
				Indent();

				switch (token.type)
				{
					case "text": {
						if (t < tokens.Length -1 && tokens[t + 1].type == "br")
						{
							// Merge with following br and skip the br token
							GenerateText(token.text + "\\n");
							t++;
						}
						else
							GenerateText(token.text);

						break;
					}
					case "br": {
						if (!_code.Collapsed)
							GenerateText("\\n");
						break;
					}

					case "twineLink": {
						GenerateLink(token, null);
						break;
					}

					case "macro": {

						// If macro is followed by a hook, capture it
						LexerToken hookToken = null;
						if (t < tokens.Length - 1 && tokens[t + 1].type == "hook")
							hookToken = tokens[t+1];

						bool hookConsumed = false;

						switch (token.name) {
							case "link":
							case "linkrepeat":
							case "linkreveal":
							case "linkgoto":
								hookConsumed = GenerateLink(token, hookToken);
								break;

							case "if":
							case "elseif":
							case "else":
								GenerateConditional(token, hookToken);
								hookConsumed = hookToken != null;
								break;
							
							default:
								hookConsumed = GenerateMacro(token, hookToken);
								break;
						}

						// Skip hook because it was conusmed
						if (hookConsumed)
							t++;

						break;
					}

					case "hook": {
						break;
					}
				}
			}
		}

		void GenerateText(string text)
		{
			_code.Buffer.AppendFormat("yield return new TwineText(\"{0}\");\n",
				text.Replace("\"", "\\\"")
			);
		}

		bool GenerateLink(LexerToken linkToken, LexerToken hookToken)
		{
			string name = "null";
			string text = "null";
			string passage = "null";
			string action = "null";
			bool hookConsumed = false;

			if (linkToken.type == "twineLink")
			{
				text = "@\"" + linkToken.innerText + "\"";
				passage = "@\"" + linkToken.passage + "\"";
			}
			else
			{
				// Extract text and passage from tokens
				int start = 1;
				int end = start;
				for (; end < linkToken.tokens.Length; end++)
					if (linkToken.tokens[end].type == "comma")
						break;
				text = GenerateExpression(linkToken.tokens.Skip(start).Take(end - start).ToArray());

				start = ++end;
				for (; end < linkToken.tokens.Length; end++)
					if (linkToken.tokens[end].type == "comma")
						break;
				passage = GenerateExpression(linkToken.tokens.Skip(start).Take(end - start).ToArray());

				// Handle Harlowe hooks as fragments
				if (hookToken != null && linkToken.name != "linkgoto")
				{
					action = GenerateFragment(hookToken.tokens);
					hookConsumed = true;
				}
			}

			_code.Buffer.AppendFormat(@"yield return new TwineLink({0}, {1}, {2}, {3});",
				name,
				text,
				passage,
				action
			);

			_code.Buffer.AppendLine();

			return hookConsumed;
		}

		void GenerateConditional(LexerToken macroToken, LexerToken hookToken)
		{
			if (hookToken == null)
				throw new TwineImportException("'" + macroToken.name + "' must be followed by a Harlowe-hook.");
	
			_code.Buffer.Append(macroToken.name == "elseif" ? "else if" : macroToken.name);

			if (macroToken.name != "else")
				_code.Buffer
					.Append("(")
					.Append(GenerateExpression(macroToken.tokens.Skip(1).ToArray()))
					.AppendLine(") {");
			else
				_code.Buffer
					.AppendLine("{");

			_code.Indent++;

			GenerateBody(hookToken.tokens);

			_code.Indent--;

			Indent();

			_code.Buffer
				.AppendLine("}");

		}

		bool GenerateMacro(LexerToken macroToken, LexerToken hookToken)
		{
			return false;
		}

		string GenerateFragment(LexerToken[] tokens)
		{
			Code outer = _code;
			_code = new Code();
			GenerateBody(tokens);
			_output.Fragments.Add(_code.Buffer.ToString());
			_code = outer;
			return string.Format("passage{0}_Fragment_{1}", _input.Pid, _output.Fragments.Count - 1);
		}

		string GenerateExpression(LexerToken[] tokens, string @default = "null")
		{
			var expr = new StringBuilder();
			for (int t = 0; t < tokens.Length; t++)
			{
				LexerToken token = tokens[t];
				expr.Append(GetExpressionSegment(token));
			}
			
			return expr.Length > 0 ?
				expr.ToString() :
				@default;
		}

		string GetExpressionSegment(LexerToken token)
		{
			switch (token.type)
			{
				case "variable":
					Importer.RegisterVar(token.name);
					return token.name;
				case "and":
					return "&&";
				case "or":
					return "||";
				case "is":
					return "==";
				case "to":
					return "=";
				case "not":
					return "!";
				default:
					return token.text;
			}
		}

		void Indent()
		{
			Utils.CodeGen.Indent(_code.Indent, _code.Buffer);
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