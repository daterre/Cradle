using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UnityTwine.Editor.StoryFormats.Sugar
{
	public delegate CodeGenMacroOutput CodeGenMacro(SugarTranscoder parser, string macro, string argument);

	public struct CodeGenMacroOutput
	{
		public int IndentChangeBefore;
		public int IndentChangeAfter;
		public string Code;

		public CodeGenMacroOutput(string code, int indentChangeBefore = 0, int indentChangeAfter = 0)
		{
			this.IndentChangeBefore = indentChangeBefore;
			this.IndentChangeAfter = indentChangeAfter;
			this.Code = code;
		}

		public static implicit operator CodeGenMacroOutput(string code)
		{
			return new CodeGenMacroOutput(code);
		}
		
	}

	public static class BuiltInCodeGenMacros
	{
		static Regex rx_params = new Regex("\"([^\"]*)\"|'([^']*)'|([^\\s]+)");

		static string ParamsWithCommas(string rawParams)
		{
			string output = string.Empty;
			MatchCollection matches = rx_params.Matches(rawParams);
			for (int i = 0; i < matches.Count; i++)
			{
				output += matches[i].Value;
				if (i < matches.Count - 1)
					output += ", ";
			}

			return output;
		}

		// ......................
		public static CodeGenMacro Set = (parser, macro, argument) =>
		{
			return string.Format("{0};", parser.ParseVars(argument));
		};

		// ......................
		public static CodeGenMacro Display = (parser, macro, argument) =>
		{
			return string.Format("yield return new TwineDisplay({0});", parser.ParseVars(argument));
		};
		
		// ......................

		public static CodeGenMacro DisplayShorthand = (parser, macro, argument) =>
		{
			string passageExpr = parser.ParseVars(macro);
			string args = argument != null ? parser.ParseVars(argument) : null;
			if (!string.IsNullOrEmpty(args))
				args = ", " + ParamsWithCommas(args);
			return string.Format("yield return new TwineDisplay(\"{0}\"{1});", passageExpr, args);
		};
		// ......................
		public static CodeGenMacro Print = (parser, macro, argument) =>
		{
			return string.Format("yield return new TwineText({0});", parser.ParseVars(argument));
		};
		
		// ......................
		
		public static CodeGenMacro Conditional = (parser, macro, argument) =>
		{
			CodeGenMacroOutput logic;
			bool format = false;

			switch (macro)
			{
				case "if":
					logic = "if ({0}) {{";
					logic.IndentChangeAfter = 1;
					format = true;
					break;
				case "elseif":
					logic = "}} else if ({0}) {{";
					logic.IndentChangeBefore = -1;
					logic.IndentChangeAfter = 1;
					format = true;
					break;
				case "else":
					logic = "} else {";
					logic.IndentChangeBefore = -1;
					logic.IndentChangeAfter = 1;
					break;
				case "/if":
				case "endif":
					logic = "}";
					logic.IndentChangeBefore = -1;
					break;
				default:
					throw new FormatException("Conditional macro doesn't support " + macro);
			}

			if (format)
				logic.Code = string.Format(logic.Code, parser.ParseVars(argument));

			return logic;
		};
	}
}