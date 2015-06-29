using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UnityTwine.Editor
{
	public delegate string TweeMacroParser(TweeParser parser, string macro, string argument);

	public static class TweeBuiltinMacroParsers
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
		public static TweeMacroParser Set = (parser, macro, argument) =>
		{
			return string.Format("{0};", parser.ParseVars(argument));
		};

		// ......................
		public static TweeMacroParser Display = (parser, macro, argument) =>
		{
			return string.Format("yield return new TwineDisplay({0});", parser.ParseVars(argument));
		};
		
		// ......................

		public static TweeMacroParser DisplayShorthand = (parser, macro, argument) =>
		{
			string passageExpr = parser.ParseVars(macro);
			string args = argument != null ? parser.ParseVars(argument) : null;
			if (args != null)
				args = ", " + ParamsWithCommas(args);
			return string.Format("yield return new TwineDisplay(\"{0}\"{1});", passageExpr, args);
		};
		// ......................
		public static TweeMacroParser Print = (parser, macro, argument) =>
		{
			return string.Format("yield return new TwineText({0});", parser.ParseVars(argument));
		};
		
		// ......................
		static Regex rx_Operator = new Regex(
			@"\b(and|or|is|to|not)\b(?=([^""]*""[^""]*"")*[^""]*$)",
			RegexOptions.Singleline | RegexOptions.Multiline
		);

		public static TweeMacroParser IfElse = (parser, macro, argument) =>
		{
			string statement;

			switch (macro)
			{
				case "else": return "} else { ";
				case "endif": return "} ";
				case "if": statement = "if ({0}) {{ "; break;
				case "elseif": statement = "}} else if ({0}) {{ "; break;
				default: throw new Exception("TweeMacroIfElse doesn't support " + macro);
			}

			string condition = parser.ParseVars(argument);
			condition = rx_Operator.Replace(condition, op =>
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

			return String.Format(statement, condition);
		};
	}
}
