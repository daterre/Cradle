using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Cradle.Editor.Importers;
using Cradle.Editor.Utils;
using Cradle.StoryFormats.Harlowe;

namespace Cradle.Editor.StoryFormats.Harlowe
{
	[InitializeOnLoad]
	public class HarloweTranscoder : StoryFormatTranscoder
	{
		static Dictionary<string, HarloweCodeGenMacro> CodeGenMacros = new Dictionary<string, HarloweCodeGenMacro>(StringComparer.OrdinalIgnoreCase);
		public GeneratedCode Code { get; private set; }
		HarlowePassageData _input;
		PassageCode _output;
		string _lastVariable;
		internal int StyleCounter;

		static HarloweTranscoder()
		{
			TwineHtmlImporter.RegisterTranscoder<HarloweTranscoder>(weight: 100);

			// Supported code generation macros
			CodeGenMacros["put"] =
			CodeGenMacros["move"] = 
			CodeGenMacros["set"] = BuiltInCodeGenMacros.Assignment;

			CodeGenMacros["unless"] =
			CodeGenMacros["if"] =
			CodeGenMacros["elseif"] =
			CodeGenMacros["else"] = BuiltInCodeGenMacros.Conditional;

			CodeGenMacros["link"] =
			CodeGenMacros["linkreplace"] =
			CodeGenMacros["linkgoto"] =
			CodeGenMacros["linkreveal"] =
			CodeGenMacros["linkrepeat"] = BuiltInCodeGenMacros.Link;

			CodeGenMacros["goto"] = BuiltInCodeGenMacros.GoTo;
			CodeGenMacros["print"] = BuiltInCodeGenMacros.Print;
			CodeGenMacros["display"] = BuiltInCodeGenMacros.Display;

            CodeGenMacros["replace"] =
			CodeGenMacros["append"] =
			CodeGenMacros["prepend"] = BuiltInCodeGenMacros.Enchant;
            
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
            CodeGenMacros["mouseoutprepend"] = BuiltInCodeGenMacros.EnchantIntoLink;

			CodeGenMacros["live"] = BuiltInCodeGenMacros.Live;
			CodeGenMacros["stop"] = BuiltInCodeGenMacros.Stop;

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
				StoryBaseType = typeof(Cradle.StoryFormats.Harlowe.HarloweStory),
				StrictMode = true
			};
		}

		public override bool RecognizeFormat()
		{
			// If it's not a Twine HTML file, ignore the asset
			if (!(this.Importer is TwineHtmlImporter))
				return false;

			// Otherwise, return true - that means, Harlowe is the default format
			// (the bridge script should return an error if it's not actually Harlowe)
			return true;
		}

		public override void Init()
		{
			// Run the story file in PhantomJS, inject the bridge script that invokes the Harlowe lexer and deserialize the JSON output
			PhantomOutput<HarloweStoryData> output;

			try
			{
				output = PhantomJS.Run<HarloweStoryData>(
					new System.Uri(Application.dataPath + "/../" + Importer.AssetPath).AbsoluteUri,
					Application.dataPath + "/Cradle/Editor/js/StoryFormats/Harlowe/harlowe.bridge.js_"
				);
			}
			catch(StoryImportException)
			{
				throw new StoryImportException("HTML or JavaScript errors encountered in the Harlowe story. Does it load properly in a browser?");
			}

			// Add the passages to the importer
			this.Importer.Passages.AddRange(output.result.passages);

			// Add the start passage to the metadata
			this.Importer.Metadata.StartPassage = output.result.passages
				.Where(p => p.Pid == output.result.startPid)
				.Select(p => p.Name)
				.FirstOrDefault();
		}

		public override PassageCode PassageToCode(PassageData p)
		{
			if (p is HarlowePassageData == false)
				throw new NotSupportedException("HarloweTranscoder called with incompatible passage data");

			StyleCounter = 0;

			_input = (HarlowePassageData)p;
			_output = new PassageCode();
			Code = new GeneratedCode();

			GenerateBody(_input.Tokens);

			// Get final string
			string code = Code.Buffer.ToString();
			_output.Main = code;

			return _output;
		}

		public void GenerateBody(LexerToken[] tokens, bool breaks = true)
		{
			if (tokens != null)
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
						case "heading":
							Code.Indent();
							GenerateStyleScope(string.Format("\"{0}\", {1}", token.type, token.depth), token.tokens);
							break;
						case "italic":
						case "bold":
						case "em":
						case "del":
						case "strong":
						case "sup":
							Code.Indent();
							GenerateStyleScope(string.Format("\"{0}\", true", token.type), token.tokens);
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
								GenerateStyleScope(BuildVariableRef(token), tokens[hookIndex].tokens, true);
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
							GenerateStyleScope(string.Format("\"hook\", \"{0}\"", token.name), tokens[t].tokens);
							break;
						default:
							break;
					}
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

		public void GenerateStyleScope(string styleParams, LexerToken[] tokens, bool conditional = false)
		{
			Code.Indent();
			if (conditional)
				Code.Buffer.AppendFormat("var styl{0} = style({1}); if (styl{0}) using (Group(styl{0})) {{", ++StyleCounter, styleParams);
			else
				Code.Buffer.AppendFormat("using (Group({0})) {{", styleParams);

			Code.Buffer.AppendLine();
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
			Code.Collapsed = outer.Collapsed; // inherit collpased whitespace setting
			GenerateBody(tokens, true );
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

			throw new StoryFormatTranscodeException(string.Format("The '{0}' assignment was not written correctly.", assignType));
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

			throw new StoryFormatTranscodeException(string.Format("{0} macro was not formatted correctly", moveAssignment ? "Move" : "Put"));
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
					if (token.text == "time")
						return "this.PassageTime";
					if (_lastVariable == null)
						throw new StoryFormatTranscodeException("'it' or 'its' used without first mentioning a variable");
					return _lastVariable;
				case "macro":
					GenerateMacro(tokens, tokenIndex, MacroUsage.Inline);
					return null;
				case "hookRef":
					return string.Format("hookRef(\"{0}\")", token.name);
				case "string":
					return WrapInVarIfNecessary(string.Format("\"{0}\"", token.innerText), tokens, tokenIndex);
				case "number":
					return WrapInVarIfNecessary(token.text, tokens, tokenIndex);
				case "cssTime":
					return WrapInVarIfNecessary(string.Format("{0}/1000f", token.value), tokens, tokenIndex);
				case "colour":
					return WrapInVarIfNecessary(string.Format("\"{0}\"", token.text), tokens, tokenIndex);
				case "text":
					return token.text == "null" ? "StoryVar.Empty" : token.text;
				case "grouping":
					if(IsWrapInVarRequired(tokens, tokenIndex))
						Code.Buffer.Append(" v");
					Code.Buffer.Append("(");
					GenerateExpression(token.tokens);
					Code.Buffer.Append(")");
					return null;
				case "itsProperty":
					if (_lastVariable == null)
						throw new StoryFormatTranscodeException("'it' or 'its' used without first mentioning a variable");
					return string.Format("{0}[\"{1}\"]", _lastVariable, token.name);
				case "property":
					string prop = string.Format("[\"{0}\"]", token.name);
					if (_lastVariable != null)
						_lastVariable += prop;
					return prop;
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
					throw new StoryFormatTranscodeException(string.Format("'{0}' is an assignment keyword and cannot be used inside an expression.", token.type));
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

			throw new StoryFormatTranscodeException("There is an incomplete expession in your code. Does it work in the browser?");
		}

		bool IsWrapInVarRequired(LexerToken[] tokens, int tokenIndex)
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

		string WrapInVarIfNecessary(string expr, LexerToken[] tokens, int tokenIndex)
		{
			return IsWrapInVarRequired(tokens, tokenIndex) ?
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
				throw new StoryFormatTranscodeException(string.Format(
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
		public int? depth; 
		public LexerToken[] tokens;
	}

	[Serializable]
	public class HarloweStoryData
	{
		public string startPid;
		public HarlowePassageData[] passages;
	}

	[Serializable]
	public class HarlowePassageData : PassageData
	{
		public LexerToken[] Tokens;
	}
}