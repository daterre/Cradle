#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityTwine.Editor.Importers;
using UnityTwine.Editor.Utils;
using UnityTwine.StoryFormats.Harlowe;

namespace UnityTwine.Editor.StoryFormats.Harlowe
{
	[InitializeOnLoad]
	public class HarloweTranscoder : StoryFormatTranscoder
	{
		#region Regex 
		// ---------------------------

		static Regex rx_LinkNames = new Regex(@"((?'linkName'[^|\n]+?)\s*=\s*)?(?'linkText'.*)",
			RegexOptions.IgnoreCase |
			RegexOptions.ExplicitCapture);

		// ---------------------------
		#endregion

		static Dictionary<string, HarloweCodeGenMacro> CodeGenMacros = new Dictionary<string, HarloweCodeGenMacro>(StringComparer.OrdinalIgnoreCase);
		public GeneratedCode Code { get; private set; }
		HarlowePassageData _input;
		TwinePassageCode _output;
		string _lastVariable;

		static HarloweTranscoder()
		{
			PublishedHtmlImporter.RegisterTranscoder<HarloweTranscoder>(weight: 100);

			// Supported code generation macros
			CodeGenMacros["put"] =
			CodeGenMacros["move"] = 
			CodeGenMacros["set"] = BuiltInCodeGenMacros.Assignment;

			CodeGenMacros["unless"] =
			CodeGenMacros["if"] =
			CodeGenMacros["elseif"] =
			CodeGenMacros["else"] = BuiltInCodeGenMacros.Conditional;

			CodeGenMacros["link"] =
			CodeGenMacros["linkgoto"] =
			CodeGenMacros["linkreveal"] =
			CodeGenMacros["linkrepeat"] = BuiltInCodeGenMacros.Link;

			CodeGenMacros["goto"] = BuiltInCodeGenMacros.GoTo;
			CodeGenMacros["print"] = BuiltInCodeGenMacros.Print;

            CodeGenMacros["replace"] =
			CodeGenMacros["append"] = 
            CodeGenMacros["prepend"] =
            CodeGenMacros["click"] =
            CodeGenMacros["clickreplace"] =
            CodeGenMacros["clickappend"] =
            CodeGenMacros["clickprepend"] =
            CodeGenMacros["mouseover"] =
            CodeGenMacros["mouseoverreplace"] =
            CodeGenMacros["mouseoverappend"] =
            CodeGenMacros["mouseoverprepend"] =
            CodeGenMacros["mouseout"] =
            CodeGenMacros["mouseoutreplace"] =
            CodeGenMacros["mouseoutappend"] =
            CodeGenMacros["mouseoutprepend"] = BuiltInCodeGenMacros.Enchant;

			CodeGenMacros["align"] =
			CodeGenMacros["font"] =
			CodeGenMacros["css"] =
			CodeGenMacros["background"] =
			CodeGenMacros["color"] =
				CodeGenMacros["colour"] =
				CodeGenMacros["textcolor"] =
				CodeGenMacros["textcolour"] =
			CodeGenMacros["textstyle"] =
			CodeGenMacros["textrotate"] =
			CodeGenMacros["transition"] =
				CodeGenMacros["t8n"] =
			CodeGenMacros["hook"] = BuiltInCodeGenMacros.Style;
		}

		public override StoryFormatMetadata GetMetadata()
		{
			return new StoryFormatMetadata()
			{
				StoryFormatName = "Harlowe",
				StoryBaseType = typeof(UnityTwine.StoryFormats.Harlowe.HarloweStory),
				StrictMode = true
			};
		}

		public override bool RecognizeFormat()
		{
			// If it's not an HTML file, ignore the asset
			if (!(this.Importer is PublishedHtmlImporter))
				return false;

			// Otherwise, return true - that means, Harlowe is the default format
			// (the bridge script should return an error if it's not actually Harlowe)
			return true;
		}

		public override void Init()
		{
			// Run the story file in PhantomJS, inject the bridge script that invokes the Harlowe lexer and deserialize the JSON output
			PhantomOutput<HarlowePassageData[]> output;

			try
			{
				output = PhantomJS.Run<HarlowePassageData[]>(
					new System.Uri(Application.dataPath + "/../" + Importer.AssetPath).AbsoluteUri,
					new System.Uri(Application.dataPath + "/Plugins/UnityTwine/Editor/StoryFormats/Harlowe/.js/harlowe.bridge.js").AbsolutePath
				);
			}
			catch(TwineImportException)
			{
				throw new TwineImportException("HTML or JavaScript errors encountered in the Harlowe story. Does it load properly in a browser?");
			}

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
			_output.Main = code;

			return _output;
		}

		public void GenerateBody(LexerToken[] tokens, bool breaks = true)
		{
			for (int t = 0; t < tokens.Length; t++)
			{
				LexerToken token = tokens[t];

				switch (token.type)
				{
					case "text":
						Code.Indent();
						GenerateText(token.text, true);
						break;

					case "verbatim":
						Code.Indent();
						GenerateText(token.innerText, true);
						break;

					case "bulleted":
					case "numbered":
						Debug.LogWarning("Bulleted and numbered lists not currently supported");
						break;

					case "italic":
					case "bold":
					case "em":
					case "del":
					case "strong":
					case "sup":
						Code.Indent();
						GenerateStyle(string.Format("\"{0}\", true", token.type), token.tokens);
						break;

					case "collapsed":
						bool wasCollapsed = Code.Collapsed;
						Code.Collapsed = true;
						GenerateBody(token.tokens, false);
						Code.Collapsed = wasCollapsed;
						break;

					case "br":
						if (!Code.Collapsed)
						{
							Code.Indent();
							GenerateLineBreak();
						}
						break;

					case "whitespace":
						if (!Code.Collapsed)
						{
							Code.Indent();
							GenerateText(token.text, true);
						}
						break;

					case "variable":
						Code.Indent();
						int hookIndex = FollowedBy("hook", tokens, t, true, false);
						if (hookIndex >= 0)
						{
							GenerateStyle(BuildVariableRef(token), tokens[hookIndex].tokens);
							t = hookIndex;
						}
						else
							GenerateText(BuildVariableRef(token), false);
						break;

					case "macro": {

						// Can't reference variables from other macros
						_lastVariable = null;

						// If macro is followed by a hook, tell the macro
						t = GenerateMacro(tokens, t, FollowedBy("hook", tokens, t, true, false) >= 0 ?
							MacroUsage.LineAndHook :
							MacroUsage.Line
						);
						break;
					}

                    case "hook":
                        // This is only for unhandled hooks
                        // TODO: parse the before or after name tag since the Harlowe lexer doesn't
                        GenerateStyle("\"anonymousHook\", \"true\"", tokens[t].tokens);
                        break;
					default:
						break;
				}
			}

			if (breaks)
			{
				Code.Indent();
				Code.Buffer.Append("yield break;");
			}
		}

		public void GenerateText(string text, bool isString)
		{
			Code.Buffer.Append("yield return text(");
			if (isString)
			{
				Code.Buffer.Append("\"");
				Code.Buffer.Append(text.Replace("\"", "\\\""));
				Code.Buffer.Append("\"");
			}
			else
				Code.Buffer.Append(text);

			Code.Buffer.AppendLine(");");
		}

		public void GenerateLineBreak()
		{
			Code.Buffer.AppendLine("yield return lineBreak();");
		}

		public void GenerateStyle(string styleParams, LexerToken[] tokens)
		{
			Code.Buffer
				.AppendFormat("using (Style.Apply({0})) {{", styleParams)
				.AppendLine();
			Code.Indentation++;
			GenerateBody(tokens, breaks: false);
			Code.Indentation--;
			Code.Indent();
			Code.Buffer.AppendLine("}");
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
			HarloweCodeGenMacro macro;

			if (!CodeGenMacros.TryGetValue(macroToken.name, out macro))
				macro = BuiltInCodeGenMacros.RuntimeMacro;

			if (macro != null)
			{
				if (usage != MacroUsage.Inline)
					Code.Indent();

				return macro(this, tokens, macroTokenIndex, usage);
			}
			else
				return macroTokenIndex;
		}

		public void GenerateAssignment(string assignType, LexerToken[] assignTokens, int start, int end)
		{
			string delimeter = assignType == "set" ? "to" : "into";

			int t = start;
			for (; t <= end; t++)
			{
				LexerToken token = assignTokens[t];
				if (token.type == delimeter)
				{
					bool switchSides = assignType != "set";

					int leftStart = !switchSides ? start : t + 1;
					int leftEnd = !switchSides ? t - 1 : end;
					int rightStart = !switchSides ? t + 1: start;
					int rightEnd = !switchSides ? end : t - 1;

					GenerateExpression(assignTokens, leftStart, leftEnd);
					Code.Buffer.Append(" = ");
					GenerateExpression(assignTokens, rightStart, rightEnd);
					return;
				}
			}

			throw new TwineTranscodeException(string.Format("The '{0}' assignment was not written correctly.", assignType));
		}

		string BuildInverseAssignment(LexerToken[] tokens, ref int tokenIndex, bool moveAssignment)
		{
			string statement = string.Format(".{0}(vref(Vars, \"", moveAssignment ? "MoveInto" : "PutInto");
			tokenIndex++;
			for (; tokenIndex < tokens.Length; tokenIndex++)
			{
				LexerToken token = tokens[tokenIndex];
				if (token.type == "whitespace")
					continue;

				if (token.type != "variable")
					break;

				statement += token.name + "\"))";
				return statement;
			}

			throw new TwineTranscodeException(string.Format("{0} macro was not formatted correctly", moveAssignment ? "Move" : "Put"));
		}

		public void GenerateExpression(LexerToken[] tokens, int start = 0, int end = -1)
		{
			// Skip whitespace at beginning of expression
			while (start < tokens.Length && tokens[start].type == "whitespace")
				start++;

			for (int t = start; t < (end > 0 ? end + 1 : tokens.Length); t++)
			{
				LexerToken token = tokens[t];
				string expr = BuildExpressionSegment(tokens, ref t);
				if (expr != null)
					Code.Buffer.Append(expr);
			}
		}

		string BuildVariableRef(LexerToken token)
		{
			Importer.RegisterVar(token.name);
			_lastVariable = string.Format("Vars.{0}", EscapeReservedWord (token.name));
			return _lastVariable;
		}

		string BuildExpressionSegment(LexerToken[] tokens, ref int tokenIndex)
		{
			LexerToken token = tokens[tokenIndex];
			switch (token.type)
			{
				case "variable":
					return BuildVariableRef(token);
				case "identifier":
					if (_lastVariable == null)
						throw new TwineTranscodeException("'it' or 'its' used without first mentioning a variable");
					return _lastVariable;
				case "macro":
					GenerateMacro(tokens, tokenIndex, MacroUsage.Inline);
					return null;
				case "hookRef":
					return string.Format("hookRef(\"{0}\")", token.name);
				case "string":
					return WrapInVar(string.Format("\"{0}\"", token.innerText), tokens, tokenIndex);
				case "number":
					return WrapInVar(token.text, tokens, tokenIndex);
				case "colour":
					return WrapInVar(string.Format("\"{0}\"", token.text), tokens, tokenIndex);
				case "grouping":
					if(WrapInVarRequired(tokens, tokenIndex))
						Code.Buffer.Append(" v");
					Code.Buffer.Append("(");
					GenerateExpression(token.tokens);
					Code.Buffer.Append(")");
					return null;
				case "itsProperty":
					if (_lastVariable == null)
						throw new TwineTranscodeException("'it' or 'its' used without first mentioning a variable");
					return string.Format("{0}[\"{1}\"]", _lastVariable, token.name);
				case "property":
					return string.Format("[\"{0}\"]", token.name);
				case "belongingProperty":
					Code.Buffer.AppendFormat("v(\"{0}\").AsMemberOf[", token.name);
					AdvanceToNextNonWhitespaceToken(tokens, ref tokenIndex);
					GenerateExpressionSegment(tokens, ref tokenIndex);
					Code.Buffer.Append("]");
					return null;
				case "contains":
					//FollowedBy("grouping", tokens, tokenIndex, true, true);
					//return ".Contains";
                    Code.Buffer.Append(".Contains(");
                    AdvanceToNextNonWhitespaceToken(tokens, ref tokenIndex);
                    GenerateExpressionSegment(tokens, ref tokenIndex);
                    Code.Buffer.Append(")");
                    return null;
                case "isIn":
					//FollowedBy("grouping", tokens, tokenIndex, true, true);
					//return ".ContainedBy";
                    Code.Buffer.Append(".ContainedBy(");
                    AdvanceToNextNonWhitespaceToken(tokens, ref tokenIndex);
                    GenerateExpressionSegment(tokens, ref tokenIndex);
                    Code.Buffer.Append(")");
                    return null;
				case "possessiveOperator":
					Code.Buffer.Append("[");
					AdvanceToNextNonWhitespaceToken(tokens, ref tokenIndex);
					GenerateExpressionSegment(tokens, ref tokenIndex);
					Code.Buffer.Append("]");
					return null;
				case "belongingOperator":
					Code.Buffer.Append(".AsMemberOf[");
					AdvanceToNextNonWhitespaceToken(tokens, ref tokenIndex);
					GenerateExpressionSegment(tokens, ref tokenIndex);
					Code.Buffer.Append("]");
					return null;
				case "and":
					return "&&";
				case "or":
					return "||";
				case "is":
					return "==";
				case "isNot":
					return "!=";
				case "not":
					return "!";
				case "spread":
					return "(HarloweSpread)";
				case "to":
				case "into":
					throw new TwineTranscodeException(string.Format("'{0}' is an assignment keyword and cannot be used inside an expression.", token.type));
				default:
					return token.text;
			}
		}

		void GenerateExpressionSegment(LexerToken[] tokens, ref int tokenIndex)
		{
			string segment = BuildExpressionSegment (tokens, ref tokenIndex);
			if (segment != null)
				Code.Buffer.Append (segment);
		}

		void AdvanceToNextNonWhitespaceToken(LexerToken[] tokens, ref int tokenIndex)
		{
			for (int t = tokenIndex + 1; t < tokens.Length; t++)
			{
				if (tokens[t].type == "whitespace")
					continue;

				tokenIndex = t;
				return;
			}

			throw new TwineTranscodeException("There is an incomplete expession in your code. Does it work in the browser?");
		}

		bool WrapInVarRequired(LexerToken[] tokens, int tokenIndex)
		{
			bool wrap = false;

			if (tokenIndex < tokens.Length - 1)
			{
				for (int t = tokenIndex + 1; t < tokens.Length; t++)
				{
					switch(tokens[t].type)
					{
						case "property":
						case "contains":
						case "isIn":
						case "into":
						case "possessiveOperator":
						case "belongingOperator":
							wrap = true;
							break;
						case "whitespace":
							continue;
						default:
							break;
					}
				}
			}

			return wrap;
		}

		string WrapInVar(string expr, LexerToken[] tokens, int tokenIndex)
		{
			return WrapInVarRequired(tokens, tokenIndex) ?
				string.Format("v({0})", expr) :
				expr;
		}

		int FollowedBy(string tokenType, LexerToken[] tokens, int tokenIndex, bool allowWhitespace, bool throwException)
		{
			int index = -1;

			if (tokenIndex < tokens.Length - 1)
			{
				for (int t = tokenIndex + 1; t < tokens.Length; t++)
				{
					string ttype = (tokens[t].type);
					if (ttype == tokenType)
					{
							index = t;
							break;
					}
					else
					{
						if (ttype == "whitespace")
						{
							if (!allowWhitespace)
								break;
							else
								continue;
						}
						else
							break;
					}
				}
			}

			if (throwException && index < 0)
				throw new TwineTranscodeException(string.Format(
					"'{0}' must be followed by a '{1}' ",
					tokens[tokenIndex].text,
					tokenType
				));

			return index;
		}
	}

	[Serializable]
	public class LexerToken
	{
		public string type;
		public string name;
		public string text;
		public string innerText;
		public double value;
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