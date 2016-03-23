using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UnityTwine.Editor.StoryFormats
{
	public delegate SugarCubeMacroOutput SugarCubeMacro(SugarCubeParser parser, string macro, string argument);

	public struct SugarCubeMacroOutput
	{
		public int IndentChangeBefore;
		public int IndentChangeAfter;
		public string Code;

		public SugarCubeMacroOutput(string code, int indentChangeBefore = 0, int indentChangeAfter = 0)
		{
			this.IndentChangeBefore = indentChangeBefore;
			this.IndentChangeAfter = indentChangeAfter;
			this.Code = code;
		}

		public static implicit operator SugarCubeMacroOutput(string code)
		{
			return new SugarCubeMacroOutput(code);
		}
		
	}

	public static class SugarCubeBuiltInMacros
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
		public static SugarCubeMacro Set = (parser, macro, argument) =>
		{
			return string.Format("{0};", parser.ParseVars(argument));
		};

		// ......................
		public static SugarCubeMacro Display = (parser, macro, argument) =>
		{
			return string.Format("yield return new TwineDisplay({0});", parser.ParseVars(argument));
		};
		
		// ......................

		public static SugarCubeMacro DisplayShorthand = (parser, macro, argument) =>
		{
			string passageExpr = parser.ParseVars(macro);
			string args = argument != null ? parser.ParseVars(argument) : null;
			if (!string.IsNullOrEmpty(args))
				args = ", " + ParamsWithCommas(args);
			return string.Format("yield return new TwineDisplay(\"{0}\"{1});", passageExpr, args);
		};
		// ......................
		public static SugarCubeMacro Print = (parser, macro, argument) =>
		{
			return string.Format("yield return new TwineText({0});", parser.ParseVars(argument));
		};
		
		// ......................
		
		public static SugarCubeMacro IfElse = (parser, macro, argument) =>
		{
			SugarCubeMacroOutput logic;
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
					throw new FormatException("SugarCubeMacroIfElse doesn't support " + macro);
			}

			if (format)
				logic.Code = string.Format(logic.Code, parser.ParseVars(argument));

			return logic;
		};
	}
}