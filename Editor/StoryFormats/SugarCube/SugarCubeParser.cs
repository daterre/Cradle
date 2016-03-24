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
using System.Security.Cryptography;
using UnityEngine;
using System.Collections;

namespace UnityTwine.Editor.StoryFormats
{
	public class SugarCubeParser : TwineFormatParser
	{
		public static Dictionary<string, SugarCubeMacro> MacroParsers = new Dictionary<string, SugarCubeMacro>(StringComparer.OrdinalIgnoreCase);

		static Regex rx_PassageBody = new Regex(@"
				(?'macro'
					(?>(?'open'<<))
						\s*
						((?'macroName'\/?[a-z_][a-z0-9_]*)\b\s*)?(?'macroArg'(?>(?'q'""))?.*?(?>(?'-q'"")?(?(q)(?!))))?
						\s*
					(?>(?'-open'>>))
				)

				|

				(?'link'
					(?>(?'open'\[\[))
						(((?'linkName'[^|\n]+?)\s*=\s*)?(?'linkText'.*?)\|)?(?'linkTarget'.+?)(\]\[(?'linkSetter'.*?))?
					(?>(?'-open'\]\]))
				)

				|

				((?<!\$)\$(?'nakedVar'[a-z_][a-z0-9_]*))

				|

				(?'text'(?'char'.)?)
				",
				RegexOptions.Singleline |
				RegexOptions.Multiline |
				RegexOptions.ExplicitCapture |
				RegexOptions.IgnoreCase |
				RegexOptions.IgnorePatternWhitespace
			);

		static Regex rx_Vars = new Regex(
				@"\$([a-zA-Z_][a-zA-Z0-9_]*)",
				RegexOptions.Singleline | RegexOptions.Multiline
			);
		static Regex rx_Operator = new Regex(
				@"\b(and|or|is|to|not)\b(?=([^""]*""[^""]*"")*[^""]*$)",
				RegexOptions.Singleline | RegexOptions.Multiline
			);

		static MD5 _md5 = MD5.Create();

        static SugarCubeParser()
        {
			// Supported macros
			MacroParsers["display"] = SugarCubeBuiltInMacros.Display;
			MacroParsers["set"] = SugarCubeBuiltInMacros.Set;
			MacroParsers["run"] = SugarCubeBuiltInMacros.Set;
			MacroParsers["print"] = SugarCubeBuiltInMacros.Print;
			MacroParsers["if"] = SugarCubeBuiltInMacros.IfElse;
			MacroParsers["elseif"] = SugarCubeBuiltInMacros.IfElse;
			MacroParsers["else"] = SugarCubeBuiltInMacros.IfElse;
			MacroParsers["endif"] = SugarCubeBuiltInMacros.IfElse;

			// TODO:
			MacroParsers["silently"] = null;
			MacroParsers["/silently"] = null;
			MacroParsers["endsilently"] = null;
			MacroParsers["nobr"] = null;
			MacroParsers["/nobr"] = null;
			MacroParsers["endnobr"] = null;
			MacroParsers["script"] = null;
			MacroParsers["/script"] = null;
			MacroParsers["endscript"] = null;

			// Unsupported macros. Recognize them but don't output anything
			
			MacroParsers["remember"] = null;
			MacroParsers["actions"] = null;
			MacroParsers["choice"] = null;
        }

		public SugarCubeParser(TwineImporter importer): base(importer)
		{
		}

		// Instance vars
		int _indent = 0;

		public override TwinePassageCode PassageToCode(TwinePassageData passage)
		{
			this._indent = 0;

			var textBuffer = new StringBuilder();
			var outputBuffer = new StringBuilder();
			MatchCollection matches = rx_PassageBody.Matches(passage.Body);

			foreach (Match match in matches)
			{
				if (match.Groups["text"].Success)
				{
					// .....................................
					// TEXT CHARACTER

					// Text characters are buffered independently
					string character = match.Groups["char"].Value;
					switch (character)
					{
						case "\n": character = "\\n"; break;
						case "\"": character = "\"\""; break;
					}

					textBuffer.Append(character);
				}
				else
				{
					// This is not a text match so output any text previously buffered
					OutputTextBuffer(outputBuffer, textBuffer);
				}

				if (match.Groups["macro"].Success)
				{
					// .....................................
					// MACRO

					string macroName = null;
					SugarCubeMacro parseMacro;
					if (match.Groups["macroName"].Success)
					{
						macroName = match.Groups["macroName"].Value;
						if (!MacroParsers.TryGetValue(macroName, out parseMacro))
							parseMacro = SugarCubeBuiltInMacros.DisplayShorthand;
					}
					else
					{
						parseMacro = SugarCubeBuiltInMacros.Print;
					}

					if (parseMacro != null)
					{
						// Get macro output from macro function. Includes indentation instructions
						SugarCubeMacroOutput macroOutput = parseMacro(this, macroName,
							match.Groups["macroArg"].Success ? match.Groups["macroArg"].Value : null
						);

						// Change indentation and output the code
						this._indent += macroOutput.IndentChangeBefore;
						OutputAppend(outputBuffer, macroOutput.Code);
						this._indent += macroOutput.IndentChangeAfter;
					}
				}
				else if (match.Groups["link"].Success)
				{
					// .....................................
					// LINK

					string linkTarget = match.Groups["linkTarget"].Value;

					bool hasText = match.Groups["linkText"].Success;
					string text = hasText ? match.Groups["linkText"].Value : linkTarget;
					//if (hasText)
					//{
					//	text = rx_Macro.Replace(text, m =>
					//	{
					//		if (m.Groups["macro"].Success)
					//			return null;
					//		else
					//		{
					//			Debug.Log(m.Groups["argument"].Value);
					//			return "\" + " + ParseVars(m.Groups["argument"].Value) + " + \"";
					//		}
					//	});
					//}

					string name = match.Groups["linkName"].Success ? match.Groups["linkName"].Value : text;
					string setters = match.Groups["linkSetter"].Length > 0 ?
						string.Format("() =>{{ {0}; return null; }}", ParseVars(match.Groups["linkSetter"].Value)) : // stick the setter into a lambda
						null;
					
					OutputAppend(outputBuffer, "yield return new TwineLink(@\"{0}\", @\"{1}\", {2}, {3};",
						name.Replace("\"", "\"\""),
						text.Replace("\"", "\"\""),
						linkTarget.IndexOf('(') >= 1 ? linkTarget : string.Format("@\"{0}\"", linkTarget.Replace("\"", "\"\"")), // if a peren is present, treat as a function
						setters == null ? "null" : setters
					);
				}
				else if (match.Groups["nakedVar"].Success)
				{
					// .....................................
					// NAKED VAR
					OutputAppend(outputBuffer, "yield return new TwineText({0});", match.Groups["nakedVar"].Value); ;
				}
			};

			// Output any leftover buffered text
			OutputTextBuffer(outputBuffer, textBuffer);

			// Get final string
			string output = outputBuffer.ToString();
			if (output == null || output.Trim().Length == 0)
				output = "yield break;";

			return new TwinePassageCode() { Main = output };
		}

		void OutputAppend(StringBuilder outputBuffer, string format, params object[] args)
		{
			var tabsTemp = new StringBuilder();
			for (int i = 0; i < Math.Max(0, this._indent); i++)
				tabsTemp.Append('\t');
			string tabs = tabsTemp.ToString();

			if (outputBuffer.Length > 0 && outputBuffer[outputBuffer.Length - 1] == '\n')
				outputBuffer.Append(tabs);
			string indented = Regex.Replace(format, "\\n", newline => newline.Value + tabs) + '\n';
			if (args != null && args.Length > 0)
				outputBuffer.AppendFormat(indented, args);
			else
				outputBuffer.Append(indented);
		}

		void OutputTextBuffer(StringBuilder outputBuffer, StringBuilder textBuffer)
		{
			if (textBuffer.Length > 0)
			{
				OutputAppend(outputBuffer, "yield return new TwineText(@\"{0}\");", textBuffer.ToString());
				textBuffer.Length = 0;
			}
		}

		internal string ParseVars(string expression)
		{
			string parsed = rx_Vars.Replace(expression, varName =>
			{
				string val = varName.Groups[1].Value;
				Importer.RegisterVar(val);
				return val;
			});

			return rx_Operator.Replace(parsed, op =>
			{
				switch (op.Value)
				{
					case "and": return "&&";
					case "or": return "||";
					case "is": return "==";
					case "to": return "=";
					case "not": return "!";
				};
				return string.Empty;
			});
		}   
	}
}
#endif