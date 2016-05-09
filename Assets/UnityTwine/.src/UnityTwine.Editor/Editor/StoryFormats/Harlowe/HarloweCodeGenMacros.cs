using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityTwine.StoryFormats.Harlowe;

namespace UnityTwine.Editor.StoryFormats.Harlowe
{
	public delegate int HarloweCodeGenMacro(HarloweTranscoder transcoder, LexerToken[] tokens, int macroTokenIndex, MacroUsage usage);

	public enum MacroUsage
	{
		Inline,
		Line,
		LineAndHook
	}

	public static class BuiltInCodeGenMacros
	{
		// ......................
		public static HarloweCodeGenMacro Assignment = (transcoder, tokens, tokenIndex, usage) =>
		{
			LexerToken assignToken = tokens[tokenIndex];

			if (usage == MacroUsage.Inline)
				throw new TwineTranscodeException(string.Format("'{0}' macro cannot be used inside another macro", assignToken.name));

            if (assignToken.name.ToLower() == "move")
                throw new TwineTranscodeException(string.Format("The 'move' macro is not supported. Use 'set' or 'put'.\nTo remove members from arrays and datamaps, use subtraction or subarray.", assignToken.name));

			int start = 1;
			int end = start;
			for (; end < assignToken.tokens.Length; end++)
			{
				LexerToken token = assignToken.tokens[end];
				if (token.type == "comma")
				{
					transcoder.GenerateAssignment(assignToken.name.ToLower(), assignToken.tokens, start, end - 1);
					transcoder.Code.Buffer.Append("; ");
					start = end + 1;
				}
			}
			if (start < end)
			{
				transcoder.GenerateAssignment(assignToken.name.ToLower(), assignToken.tokens, start, end - 1);
				transcoder.Code.Buffer.Append(";");
			}

			transcoder.Code.Buffer.AppendLine();
			
			return tokenIndex;
		};

		// ......................
		public static HarloweCodeGenMacro Conditional = (transcoder, tokens, tokenIndex, usage) =>
		{
			LexerToken token = tokens[tokenIndex];

			if (usage == MacroUsage.Line)
				throw new TwineTranscodeException("'" + token.name + "' must be followed by a Harlowe-hook.");
			if (usage == MacroUsage.Inline)
				throw new TwineTranscodeException("'" + token.name + "' cannot be used inline.");

			transcoder.Code.Buffer.Append(
				token.name == "elseif" ? "else if" :
				token.name == "else" ? "else" :
				"if");

			if (token.name != "else")
			{
				transcoder.Code.Buffer.Append("(");
				if (token.name == "unless")
					transcoder.Code.Buffer.Append("!(");
				transcoder.GenerateExpression(token.tokens, start: 1);
				if (token.name == "unless")
					transcoder.Code.Buffer.Append(")");
				transcoder.Code.Buffer.AppendLine(") {");
			}
			else
				transcoder.Code.Buffer.AppendLine(" {");

			transcoder.Code.Indentation++;

			// Advance to hook
			tokenIndex++;

			LexerToken hookToken = tokens[tokenIndex];
			transcoder.GenerateBody(hookToken.tokens, false);

			transcoder.Code.Indentation--;
			transcoder.Code.Indent();
			transcoder.Code.Buffer.AppendLine("}");

			// Skip any trailing line breaks and whitespace before the next else or elseif
			if (token.name != "else")
			{
				int i = tokenIndex + 1;
				while (i < tokens.Length)
				{
					if (tokens[i].type != "br" && tokens[i].type != "whitespace")
					{
						// Jump to the position before this macro. Otherwise we'll just continue as normal
						if (tokens[i].type == "macro" && (tokens[i].name == "elseif" || tokens[i].name == "else"))
							tokenIndex = i - 1;

						break;
					}
					else
						i++;
				}
			}

			return tokenIndex;
		};

		// ......................
		static string[] linkTypesWithHook = new string[] { "link", "linkreplace", "linkrepeat", "linkreveal" };

		public static HarloweCodeGenMacro Link = (transcoder, tokens, tokenIndex, usage) =>
		{
			LexerToken linkToken = tokens[tokenIndex];
			
			// Create a hook name for links that do things to hooks.
			string hookName = null;
			bool indented = false;
			if (linkTypesWithHook.Contains(linkToken.name))
			{
				if (usage != MacroUsage.LineAndHook)
					throw new TwineTranscodeException(string.Format("'{0}' macro must be followed by a Harlowe-hook", linkToken.name));

				hookName = System.Guid.NewGuid().ToString("N");

				if (linkToken.name != "linkrepeat")
				{
					transcoder.Code.Buffer.AppendFormat("using (ApplyStyle(\"hook\", hook(\"{0}\")))", hookName).AppendLine();
					transcoder.Code.Indentation++;
					transcoder.Code.Indent();
					indented = true;
				}
			}

			// Text
			transcoder.Code.Buffer.AppendFormat("yield return link(");
			int start = 1;
			int end = start;
			for (; end < linkToken.tokens.Length; end++)
				if (linkToken.tokens[end].type == "comma")
					break;
			transcoder.GenerateExpression(linkToken.tokens, start: start, end: end - 1);

			// Passage
			transcoder.Code.Buffer.Append(", ");
			start = ++end;
			for (; end < linkToken.tokens.Length; end++)
				if (linkToken.tokens[end].type == "comma")
					break;
			if (start < end)
				transcoder.GenerateExpression(linkToken.tokens, start: start, end: end - 1);
			else
				transcoder.Code.Buffer.Append("null");

			// Action
			transcoder.Code.Buffer.Append(", ");
			if (linkToken.name == "linkgoto")
			{
				transcoder.Code.Buffer.Append("null);");
			}
			else
			{
				tokenIndex++; // advance
				LexerToken hookToken = tokens[tokenIndex];

				transcoder.Code.Buffer.AppendFormat("() => enchantHook{1}(\"{0}\", HarloweEnchantCommand.Replace, ",
					hookName,
					linkToken.name == "linkreveal" ? "WithLinkText" : null
				);
				transcoder.Code.Buffer.Append(transcoder.GenerateFragment(hookToken.tokens));

				if (linkToken.name == "linkrepeat")
				{
					transcoder.Code.Buffer.Append(", wrap: true));").AppendLine();
					transcoder.Code.Indent();
					transcoder.Code.Buffer.AppendFormat("using (ApplyStyle(\"hook\", hook(\"{0}\"))) {{}}", hookName);
				}
				else
				{
					transcoder.Code.Buffer.Append("));");
				}
			}

			// Done
			transcoder.Code.Buffer.AppendLine();

			if (indented)
				transcoder.Code.Indentation--;

			return tokenIndex;
		};

		// ......................
		public static HarloweCodeGenMacro Enchant = (transcoder, tokens, tokenIndex, usage) =>
		{
            LexerToken enchantToken = tokens[tokenIndex];

			// Open
			transcoder.Code.Buffer.AppendFormat("yield return enchant(");

            // Reference
            transcoder.GenerateExpression(enchantToken.tokens, 1, enchantToken.tokens.Length-1);

			// Enchant command
			transcoder.Code.Buffer.AppendFormat(", HarloweEnchantCommand.{0}",
				enchantToken.name == "append" ?	HarloweEnchantCommand.Append :
				enchantToken.name == "prepend" ? HarloweEnchantCommand.Prepend :
				enchantToken.name == "replace" ? HarloweEnchantCommand.Replace :
				HarloweEnchantCommand.None
			);

            // Action
            transcoder.Code.Buffer.Append(", ");
            if (usage == MacroUsage.LineAndHook)
            {
                tokenIndex++; // advance
                LexerToken hookToken = tokens[tokenIndex];
                transcoder.Code.Buffer.Append(transcoder.GenerateFragment(hookToken.tokens));
            }
            else
                throw new TwineTranscodeException(string.Format("'{0}' macro must be followed by a Harlowe-hook", enchantToken.name));

            // Close
            transcoder.Code.Buffer.AppendLine(");");

            return tokenIndex;
		};

		public static HarloweCodeGenMacro EnchantIntoLink = (transcoder, tokens, tokenIndex, usage) =>
		{
			LexerToken enchantToken = tokens[tokenIndex];

			if (usage != MacroUsage.LineAndHook)
				throw new TwineTranscodeException(string.Format("'{0}' macro must be followed by a Harlowe-hook", enchantToken.name));

			// Open
			transcoder.Code.Buffer.AppendFormat("yield return enchantIntoLink(");

			// Reference
			transcoder.GenerateExpression(enchantToken.tokens, 1, enchantToken.tokens.Length - 1);

			// Action
			transcoder.Code.Buffer.Append(", ");
			tokenIndex++; // advance
			LexerToken hookToken = tokens[tokenIndex];
			LexerToken[] actionTokens;

			// This might be a double enchant
			HarloweEnchantCommand command =
				enchantToken.name.EndsWith("append") ? HarloweEnchantCommand.Append :
				enchantToken.name.EndsWith("prepend") ? HarloweEnchantCommand.Prepend :
				enchantToken.name.EndsWith("replace") ? HarloweEnchantCommand.Replace :
				HarloweEnchantCommand.None;

			if (command != HarloweEnchantCommand.None)
			{
				// Rename the enchant before reusing it
				enchantToken.name = command.ToString().ToLower();

				// Wrap the second enchant in its own fragment
				actionTokens = new LexerToken[]
				{
					enchantToken,
					hookToken
				};
			}
			else
				actionTokens = hookToken.tokens;

			transcoder.Code.Buffer.Append(transcoder.GenerateFragment(actionTokens));	

			// Close
			transcoder.Code.Buffer.AppendLine(");");

			return tokenIndex;
		};

		// ......................
		public static HarloweCodeGenMacro GoTo = (transcoder, tokens, tokenIndex, usage) =>
		{
			if (usage == MacroUsage.Inline)
				throw new TwineTranscodeException("GoTo macro cannot be used inside another macro");

			transcoder.Code.Buffer.Append("yield return abort(goToPassage: ");
			transcoder.GenerateExpression(tokens[tokenIndex].tokens, 1);
			transcoder.Code.Buffer.AppendLine(");");

			return tokenIndex;
		};

        // ......................
        public static HarloweCodeGenMacro Display = (transcoder, tokens, tokenIndex, usage) =>
        {
            if (usage == MacroUsage.Inline)
                throw new TwineTranscodeException("Display macro cannot be used inside another macro");

            transcoder.Code.Buffer.Append("yield return passage(");
            transcoder.GenerateExpression(tokens[tokenIndex].tokens, 1);
            transcoder.Code.Buffer.AppendLine(");");

            return tokenIndex;
        };

		// ......................
		public static HarloweCodeGenMacro Style = (transcoder, tokens, tokenIndex, usage) =>
		{
			LexerToken macroToken = tokens[tokenIndex];

			bool isHook = macroToken.name == "hook";
			string option = "\"" + macroToken.name + "\"";

			if (usage == MacroUsage.Inline)
			{
				transcoder.Code.Buffer.AppendFormat("style({0}, {1}", option, isHook ? "hook(" : null);
				transcoder.GenerateExpression(macroToken.tokens, start: 1);
				transcoder.Code.Buffer.AppendFormat("{0})", isHook ? ")" : null);
			}
			else if (usage == MacroUsage.LineAndHook)
			{
				transcoder.Code.Buffer.AppendFormat("using (ApplyStyle({0}, {1}", option, isHook ? "hook(" : null);
				transcoder.GenerateExpression(macroToken.tokens, start: 1);
				transcoder.Code.Buffer.AppendFormat("{0})) {{", isHook ? ")" : null);
				transcoder.Code.Buffer.AppendLine();

				// Advance to hook
				tokenIndex++;
				LexerToken hookToken = tokens[tokenIndex];

				transcoder.Code.Indentation++;
				transcoder.GenerateBody(hookToken.tokens, false);
				transcoder.Code.Indentation--;
				transcoder.Code.Indent();
				transcoder.Code.Buffer.AppendLine("}");
			}
			else
				throw new TwineTranscodeException(string.Format("The '{0}' macro must either be attached to a Harlowe-hook or assigned to a variable.", macroToken.name));		

			return tokenIndex;
		};

		// ......................
		public static HarloweCodeGenMacro Print = (transcoder, tokens, tokenIndex, usage) =>
		{
			if (usage != MacroUsage.Inline)
			{
				transcoder.Code.Buffer.Append("yield return text(");
				transcoder.GenerateExpression(tokens[tokenIndex].tokens, 1);
				transcoder.Code.Buffer.AppendLine(");");
			}
			else
				transcoder.Code.Buffer.Append("null");

			return tokenIndex;
		};

		// ......................

		public static HarloweCodeGenMacro Live = (transcoder, tokens, tokenIndex, usage) =>
		{
			LexerToken liveToken = tokens[tokenIndex];

			// Open
			transcoder.Code.Buffer.AppendFormat("yield return new HarloweLive(");

			// Reference
			transcoder.GenerateExpression(liveToken.tokens, 1, liveToken.tokens.Length - 1);

			// Action
			transcoder.Code.Buffer.Append(", ");
			if (usage == MacroUsage.LineAndHook)
			{
				tokenIndex++; // advance
				LexerToken hookToken = tokens[tokenIndex];
				transcoder.Code.Buffer.Append(transcoder.GenerateFragment(hookToken.tokens));
			}
			else
				throw new TwineTranscodeException(string.Format("'{0}' macro must be followed by a Harlowe-hook", liveToken.name));

			// Close
			transcoder.Code.Buffer.AppendLine(");");

			return tokenIndex;
		};

		public static HarloweCodeGenMacro Stop = (transcoder, tokens, tokenIndex, usage) =>
		{
			transcoder.Code.Buffer.AppendLine("yield return new HarloweLiveStop();");
			return tokenIndex;
		};

		// ......................

		static string[] UnsupportedRuntimeMacros = new string[] {
			"alert",
			"prompt",
			"confirm",
			"loadgame",
			"savegame",
			"savedgames",
			"gotourl",
			"openurl",
			"pageurl",
			"reload"
		};

		public static HarloweCodeGenMacro RuntimeMacro = (transcoder, tokens, tokenIndex, usage) =>
		{
			LexerToken macroToken = tokens[tokenIndex];
			MacroDef macroDef;
			if (!transcoder.Importer.Macros.TryGetValue(macroToken.name, out macroDef))
			{
				throw new TwineImportException(string.Format(
					"Macro '{0}' {1}. You can add it as a custom macro, please see the UnityTwine documentation page.",
					macroToken.name,
					UnsupportedRuntimeMacros.Contains(macroToken.name) ? "is not supported in UnityTwine" : "does not exist"
				));
			}

			transcoder.Code.Buffer.AppendFormat("{0}.{1}(", macroDef.Lib.Name, HarloweTranscoder.EscapeReservedWord(macroDef.Name));
			transcoder.GenerateExpression(macroToken.tokens, 1);
			transcoder.Code.Buffer.Append(")");

			if (usage != MacroUsage.Inline)
				transcoder.Code.Buffer.AppendLine(";");

			return tokenIndex;
		};
	}
}