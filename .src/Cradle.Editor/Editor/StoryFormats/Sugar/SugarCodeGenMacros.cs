using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Cradle.Editor.StoryFormats.Sugar
{
	public delegate void SugarCodeGenMacro(SugarTranscoder transcoder, string macro, string argument);

	public static class BuiltInCodeGenMacros
	{
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
		public static SugarCodeGenMacro GoTo = (transcoder, macro, argument) =>
		{
			transcoder.Code.Indent();
			transcoder.Code.Buffer
				.AppendFormat("yield return abort(goToPassage: {0});", transcoder.BuildExpression(argument))
				.AppendLine();
		};

		// ......................

		public static SugarCodeGenMacro DisplayShorthand = (transcoder, macro, argument) =>
		{
			string args = argument != null ? transcoder.BuildExpression(argument) : null;
			if (!string.IsNullOrEmpty(args))
				args = SugarTranscoder.ParamsWithCommas(args);

			transcoder.Code.Indent();

			MacroDef macroDef;
			if (transcoder.Importer.Macros.TryGetValue(macro, out macroDef))
			{
				transcoder.Code.Buffer
					.AppendFormat("{0}.{1}({2});", macroDef.Lib.Name, macroDef.Name, args)
					.AppendLine();
			}
			else
			{
				string passageExpr = transcoder.BuildExpression(macro);
				
				transcoder.Code.Buffer
					.AppendFormat("yield return passage(\"{0}\"{1});", passageExpr, (!string.IsNullOrEmpty(args) ? ", " : string.Empty) + args)
					.AppendLine();
			}
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
		public static SugarCodeGenMacro Back = (transcoder, macro, argument) =>
		{
			transcoder.Code.Indent();
			transcoder.Code.Buffer
				.AppendFormat("yield return link({0}, previous(), null);", transcoder.BuildExpression(argument))
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