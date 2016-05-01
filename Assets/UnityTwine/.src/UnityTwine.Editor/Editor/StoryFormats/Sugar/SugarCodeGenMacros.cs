using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UnityTwine.Editor.StoryFormats.Sugar
{
	//public delegate CodeGenMacroOutput SugarCodeGenMacro(SugarTranscoder parser, string macro, string argument);

	//public struct CodeGenMacroOutput
	//{
	//	public int IndentChangeBefore;
	//	public int IndentChangeAfter;
	//	public string Code;

	//	public CodeGenMacroOutput(string code, int indentChangeBefore = 0, int indentChangeAfter = 0)
	//	{
	//		this.IndentChangeBefore = indentChangeBefore;
	//		this.IndentChangeAfter = indentChangeAfter;
	//		this.Code = code;
	//	}

	//	public static implicit operator CodeGenMacroOutput(string code)
	//	{
	//		return new CodeGenMacroOutput(code);
	//	}
		
	//}

	public delegate void SugarCodeGenMacro(SugarTranscoder transcoder, string macro, string argument);

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
		public static SugarCodeGenMacro Silent = (transcoder, macro, argument) =>
		{
			transcoder.NoOutput = macro == "silently";
		};

		// ......................
		public static SugarCodeGenMacro Collapse = (transcoder, macro, argument) =>
		{
			transcoder.Code.Collapsed = macro == "nobr";
		};

		// ......................
		public static SugarCodeGenMacro LineBreak = (transcoder, macro, argument) =>
		{
			transcoder.Code.Indent();
			transcoder.Code.Buffer
				.AppendLine("yield return lineBreak();");
		};

		// ......................
		public static SugarCodeGenMacro Assignment = (transcoder, macro, argument) =>
		{
			transcoder.Code.Indent();
			transcoder.Code.Buffer
				.AppendFormat("{0};", transcoder.BuildExpression(argument))
				.AppendLine();
		};

		// ......................
		public static SugarCodeGenMacro Display = (transcoder, macro, argument) =>
		{
			transcoder.Code.Indent();
			transcoder.Code.Buffer
				.AppendFormat("yield return passage({0});", transcoder.BuildExpression(argument))
				.AppendLine();
		};
		
		// ......................

		public static SugarCodeGenMacro DisplayShorthand = (transcoder, macro, argument) =>
		{
			string passageExpr = transcoder.BuildExpression(macro);
			string args = argument != null ? transcoder.BuildExpression(argument) : null;
			if (!string.IsNullOrEmpty(args))
				args = ", " + ParamsWithCommas(args);

			transcoder.Code.Indent();
			transcoder.Code.Buffer
				.AppendFormat("yield return passage(\"{0}\"{1});", passageExpr, args)
				.AppendLine();
		};
		// ......................
		public static SugarCodeGenMacro Print = (transcoder, macro, argument) =>
		{
			transcoder.Code.Indent();
			transcoder.Code.Buffer
				.AppendFormat("yield return text({0});", transcoder.BuildExpression(argument))
				.AppendLine();
		};
		
		// ......................
		public static SugarCodeGenMacro Conditional = (transcoder, macro, argument) =>
		{
			switch (macro)
			{
				case "if":
					transcoder.Code.Indent();
					transcoder.Code.Buffer
						.AppendFormat("if ({0})", transcoder.BuildExpression(argument))
						.AppendLine();
					transcoder.Code.Indent();
					transcoder.Code.Buffer.AppendLine("{");
					transcoder.Code.Indentation++;
					break;
				case "elseif":
					transcoder.Code.Indentation--;
					transcoder.Code.Indent();
					transcoder.Code.Buffer.AppendLine("}");
					transcoder.Code.Indent();
					transcoder.Code.Buffer
						.AppendFormat("else if ({0})", transcoder.BuildExpression(argument))
						.AppendLine();
					transcoder.Code.Indent();
					transcoder.Code.Buffer.AppendLine("{");
					transcoder.Code.Indentation++;
					break;
				case "else":
					transcoder.Code.Indentation--;
					transcoder.Code.Indent();
					transcoder.Code.Buffer.AppendLine("}");
					transcoder.Code.Indent();
					transcoder.Code.Buffer.AppendLine("else");
					transcoder.Code.Indent();
					transcoder.Code.Buffer.AppendLine("{");
					transcoder.Code.Indentation++;
					break;
				case "/if":
				case "endif":
					transcoder.Code.Indentation--;
					transcoder.Code.Indent();
					transcoder.Code.Buffer.AppendLine("}");
					break;
				default:
					throw new FormatException("Conditional macro doesn't support " + macro);
			}
		};

		// ......................
		public static SugarCodeGenMacro Loop = (transcoder, macro, argument) =>
		{
			switch (macro)
			{
				case "continue":
				case "break":
					transcoder.Code.Indent();
					transcoder.Code.Buffer
						.Append(macro)
						.AppendLine(";");
					break;
				case "for":
					transcoder.Code.Indent();
					transcoder.Code.Buffer
						.AppendFormat("for ({0})", transcoder.BuildExpression(argument))
						.AppendLine();
					transcoder.Code.Indent();
					transcoder.Code.Buffer.AppendLine("{");
					transcoder.Code.Indentation++;
					break;
				case "/for":
				case "endfor":
					transcoder.Code.Indentation--;
					transcoder.Code.Indent();
					transcoder.Code.Buffer.AppendLine("}");
					break;
				default:
					throw new FormatException("Loop macro doesn't support " + macro);
			}
		};
	}
}