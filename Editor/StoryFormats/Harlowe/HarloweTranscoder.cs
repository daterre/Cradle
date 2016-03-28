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
	public class HarloweTranscoder : TwineFormatTranscoder
	{
		public static Dictionary<string, CodeGenMacro> CodeGenMacros = new Dictionary<string, CodeGenMacro>(StringComparer.OrdinalIgnoreCase);

		HarlowePassageData _input;
		TwinePassageCode _output;
		string _lastVariable;

		public GeneratedCode Code { get; private set; }

		static Regex rx_LinkNames = new Regex(@"((?'linkName'[^|\n]+?)\s*=\s*)?(?'linkText'.*)",
			RegexOptions.IgnoreCase |
			RegexOptions.ExplicitCapture);

		static HarloweTranscoder()
		{
			// Supported macros
			CodeGenMacros["set"] = BuiltInCodeGenMacros.Set;

			CodeGenMacros["if"] =
			CodeGenMacros["elseif"] =
			CodeGenMacros["else"] = BuiltInCodeGenMacros.Conditional;

			CodeGenMacros["link"] =
			CodeGenMacros["linkgoto"] =
			CodeGenMacros["linkreveal"] =
			CodeGenMacros["linkrepeat"] = BuiltInCodeGenMacros.Link;

			CodeGenMacros["print"] = BuiltInCodeGenMacros.Print;
		}

		public HarloweTranscoder(TwineImporter importer) : base(importer)
		{
		}

		public override string RuntimeMacrosClassName
		{
			get { return typeof(UnityTwine.StoryFormats.Harlowe.HarloweRuntimeMacros).FullName; }
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
			Code = new GeneratedCode();

			GenerateBody(_input.Tokens);

			// Get final string
			string code = Code.Buffer.ToString();
			if (code == null || code.Trim().Length == 0)
				code = "yield break;";
			_output.Main = code;

			return _output;
		}

		public void GenerateBody(LexerToken[] tokens)
		{
			for (int t = 0; t < tokens.Length; t++)
			{
				LexerToken token = tokens[t];
				Code.Indent();

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
						if (!Code.Collapsed)
							GenerateText("\\n");
						break;
					}

					case "macro": {

						// Can't reference variables from other macros
						_lastVariable = null;

						// If macro is followed by a hook, tell the macro
						bool followedByHook = t < tokens.Length - 1 && tokens[t + 1].type == "hook";
						t = GenerateMacro(tokens, t, followedByHook ?
							MacroUsage.LineAndHook :
							MacroUsage.Line
						);
						break;
					}

					case "hook": {
						break;
					}
				}
			}
		}

		public void GenerateText(string text)
		{
			Code.Buffer.AppendFormat("yield return new TwineText(\"{0}\");\n",
				text.Replace("\"", "\\\"")
			);
		}

		public string GenerateFragment(LexerToken[] tokens)
		{
			GeneratedCode outer = Code;
			Code = new GeneratedCode();
			GenerateBody(tokens);
			_output.Fragments.Add(Code.Buffer.ToString());
			Code = outer;
			return string.Format("passage{0}_Fragment_{1}", _input.Pid, _output.Fragments.Count - 1);
		}

		public int GenerateMacro(LexerToken[] tokens, int macroTokenIndex, MacroUsage usage)
		{
			LexerToken macroToken = tokens[macroTokenIndex];
			CodeGenMacro macro;

			if (!CodeGenMacros.TryGetValue(macroToken.name, out macro))
				macro = BuiltInCodeGenMacros.RuntimeMacro;

			if (macro != null)
			{
				Code.Indent();
				return macro(this, tokens, macroTokenIndex, usage);
			}
			else
				return macroTokenIndex;
		}

		public void GenerateExpression(LexerToken[] tokens, int start = 0, int end = -1)
		{
			// Skip whitespace at beginning of expression
			while (start < tokens.Length && tokens[start].type == "whitespace")
				start++;

			for (int t = start; t < (end > 0 ? end + 1 : tokens.Length); t++)
			{
				LexerToken token = tokens[t];
				string expr = BuildExpressionSegment(tokens, t);
				if (expr != null)
					Code.Buffer.Append(expr);
			}
		}

		string BuildExpressionSegment(LexerToken[] tokens, int tokenIndex)
		{
			LexerToken token = tokens[tokenIndex];
			switch (token.type)
			{
				case "variable":
					Importer.RegisterVar(token.name);
					_lastVariable = token.name;
					return token.name;
				case "identifiter":
					if (_lastVariable == null)
						throw new TwineTranscodingException("'it' or 'its' used without first mentioning a variable");
					return token.name;
				case "macro":
					GenerateMacro(tokens, tokenIndex, MacroUsage.Inline);
					return null;
				case "grouping":
					Code.Buffer.Append("(");
					GenerateExpression(token.tokens);
					Code.Buffer.Append(")");
					return null;
				case "property":
					return string.Format("[\"{0}\"]", token.name);
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

	public class GeneratedCode
	{
		public StringBuilder Buffer = new StringBuilder();
		public int Indentation = 0;
		public bool Collapsed = false;

		public void Indent()
		{
			Utils.CodeGen.Indent(Indentation, Buffer);
		}
	}
}
#endif