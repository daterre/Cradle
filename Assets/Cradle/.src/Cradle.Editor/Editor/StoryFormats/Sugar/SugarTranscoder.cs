using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using Cradle.Editor.Importers;
using Cradle.Editor.Utils;
using Cradle.StoryFormats.Sugar;

namespace Cradle.Editor.StoryFormats.Sugar
{
	[InitializeOnLoad]
	public class SugarTranscoder : StoryFormatTranscoder
	{
		#region Regex
		// ---------------------------

		static Regex rx_Sniff = new Regex(@"((<!--\s*?SugarCube)|(<!--.*?Sugarcane))",
			RegexOptions.Singleline |
			RegexOptions.IgnoreCase |
			RegexOptions.ExplicitCapture);

		const string RX_NOQUOTES = @"(?=([^""\\]*(\\.|""([^""\\]*\\.)*[^""\\]*""))*[^""]*$)";

		static Regex rx_PassageBody = new Regex(@"
				(?'macro'
					<<
						\s*
						((?'macroName'\/?[a-z_=\-][a-z0-9_]*)\s*)?
						(?'macroArg'
							(?>(?'quote'""))?
							.*?
							(?>(?'close-quote'"")?(?(quote)(?!)))
						)?
						\s*
					>>
				)

				|

				(?'link'
					\[
						(?>(?'open'\[))
							((?'linkText'.*?)\|)?
							(?'linkTarget'.+?)
						(?>(?'close-open'\])(?(open)(?!)))
						(
							(?>(?'open'\[))
								(?'linkSetter'.*)
							(?>(?'close-open'\])(?(open)(?!)))
						)?
					\]
				)

				|

				((?<!\$)\$(?'nakedVar'[a-z_][a-z0-9_]*))

				|

				(?'br'(\r\n|\n|\r|$))

				|

				((?'text'.+?)(?=(<<|\[\[|\$|\r|\n|$)))
				",
				RegexOptions.Multiline |
				RegexOptions.ExplicitCapture |
				RegexOptions.IgnoreCase |
				RegexOptions.IgnorePatternWhitespace
			);

		static Regex rx_Vars = new Regex(string.Format(@"\$([a-zA-Z_][a-zA-Z0-9_]*){0}", RX_NOQUOTES),
				RegexOptions.Singleline | RegexOptions.Multiline
			);

		static Regex rx_params = new Regex("\"([^\"]*)\"|'([^']*)'|([^\\s]+)");

		static Regex rx_Operator = new Regex(string.Format(
				@"\b(and|or|is not|is|to|not|eq|gt|gte|lt|lte)\b{0}", RX_NOQUOTES),
				RegexOptions.Singleline | RegexOptions.Multiline
			);

//		static Regex rx_Array = new Regex(string.Format(@"
//				(?<=\s*(?:^|[=\-\+\*%&|\(])\s*)
//				(?'array'
//					(?'argsOpen'\[)+
//					.*?{0}
//					(?'args-argsOpen'\])+
//					(?(argsOpen)(?!))
//				)+"
//				, RX_NOQUOTES),
//				RegexOptions.ExplicitCapture |
//				RegexOptions.IgnorePatternWhitespace
//			);

		// ---------------------------
		#endregion
	
		public static Dictionary<string, SugarCodeGenMacro> CodeGenMacros = new Dictionary<string, SugarCodeGenMacro>(StringComparer.OrdinalIgnoreCase);
		public GeneratedCode Code { get; private set; }
		PassageData _input;
		PassageCode _output;
		internal bool NoOutput = false;

        static SugarTranscoder()
        {
			TwineHtmlImporter.RegisterTranscoder<SugarTranscoder>();

			CodeGenMacros["set"] = 
			CodeGenMacros["run"] = BuiltInCodeGenMacros.Assignment;
			
			CodeGenMacros["if"] = 
			CodeGenMacros["elseif"] =
			CodeGenMacros["else"] =
			CodeGenMacros["/if"] = 
			CodeGenMacros["endif"] = BuiltInCodeGenMacros.Conditional;

			CodeGenMacros["for"] =
			CodeGenMacros["/for"] =
			CodeGenMacros["continue"] =
			CodeGenMacros["break"] = 
			CodeGenMacros["endfor"] = BuiltInCodeGenMacros.Loop;

			CodeGenMacros["silently"] =
			CodeGenMacros["/silently"] =
			CodeGenMacros["endsilently"] = BuiltInCodeGenMacros.Silent;
			CodeGenMacros["nobr"] =
			CodeGenMacros["/nobr"] =
			CodeGenMacros["endnobr"] = BuiltInCodeGenMacros.Collapse;
			CodeGenMacros["br"] = BuiltInCodeGenMacros.LineBreak;

			CodeGenMacros["display"] = BuiltInCodeGenMacros.Display;
			CodeGenMacros["goto"] = BuiltInCodeGenMacros.GoTo;
			
			CodeGenMacros["="] = 
			CodeGenMacros["print"] = BuiltInCodeGenMacros.Print;

			CodeGenMacros["back"] =
			CodeGenMacros["return"] = BuiltInCodeGenMacros.Back;

			// Unsupported macros. Recognize them but don't output anything	
			CodeGenMacros["remember"] = null;
			CodeGenMacros["actions"] = null;
			CodeGenMacros["choice"] = null;
			CodeGenMacros["script"] = null;
			CodeGenMacros["/script"] = null;
			CodeGenMacros["endscript"] = null;
        }

		public override StoryFormatMetadata GetMetadata()
		{
			return new StoryFormatMetadata()
			{
				StoryFormatName = "Sugar",
				StoryBaseType = typeof(SugarStory),
				StrictMode = false
			};
		}

		public override bool RecognizeFormat()
		{
			// Validate content only when using the HTML importer
			if (!(this.Importer is TwineHtmlImporter))
				return true;

			// Load the first 2KB of the file, because Sugar themes place a header there
			var headerBuffer = new char[2048];
			using (var stream = new StreamReader(Importer.AssetPath))
				stream.Read(headerBuffer, 0, headerBuffer.Length);
			string header = new String(headerBuffer);

			// It's relevant if there's a match
			return rx_Sniff.IsMatch(header);
		}

		public override void Init()
		{
			if (!(this.Importer is TwineHtmlImporter))
				return;

			// Run the story file in PhantomJS, inject the bridge script and deserialize the JSON output
			PhantomOutput<SugarStoryData> output;

			try
			{
				output = PhantomJS.Run<SugarStoryData>(
					new System.Uri(Application.dataPath + "/../" + Importer.AssetPath).AbsoluteUri,
					Application.dataPath + "/Cradle/Editor/js/StoryFormats/Sugar/sugar.bridge.js_"
				);
			}
			catch (StoryImportException)
			{
				throw new StoryImportException("HTML or JavaScript errors encountered in the Sugar story. Does it load properly in a browser?");
			}

			// Add the passages to the importer
			this.Importer.Passages.AddRange(output.result.passages);

			// Add the start passage to the metadata
			this.Importer.Metadata.StartPassage = output.result.passages
				.Where(p => p.Pid == output.result.startPid)
				.Select(p => p.Name)
				.FirstOrDefault();
		}

		public override PassageCode PassageToCode(PassageData passage)
		{
			_input = passage;
			_output = new PassageCode();

			// Ignore script and stylesheet passages
			if (passage.Tags.Contains("script") || passage.Tags.Contains("stylesheet"))
			{
				_output.Main = "yield break;";
				return _output;
			}

			Code = new GeneratedCode();
			NoOutput = false;
			if (passage.Tags.Contains("nobr"))
				Code.Collapsed = true;

			MatchCollection matches = rx_PassageBody.Matches(_input.Body);
			GenerateBody(matches);

			// Get final string
			string code = Code.Buffer.ToString();
			_output.Main = code;

			return _output;
		}

		void GenerateBody(MatchCollection matches)
		{
			foreach (Match match in matches)
			{
				// Text
				if (match.Groups["text"].Success)
				{
					if (!NoOutput)
					{
						string text = match.Groups["text"].Value
							.Replace("\"", "\"\"");

						if (Code.Collapsed)
							text = Regex.Replace(text, @"(\s)+", " ");

						Code.Indent();
						Code.Buffer
							.AppendFormat("yield return text(@\"{0}\");", text)
							.AppendLine();
					}
				}

				// Line break
				else if (match.Groups["br"].Success)
				{
					if (!Code.Collapsed && !NoOutput)
					{
						Code.Indent();
						Code.Buffer
							.AppendLine("yield return lineBreak();");
					}
				}

				// Macro
				else if (match.Groups["macro"].Success)
				{
					string macroName = null;
					SugarCodeGenMacro macro;
					if (match.Groups["macroName"].Success)
					{
						macroName = match.Groups["macroName"].Value;
						if (!CodeGenMacros.TryGetValue(macroName, out macro))
							macro = BuiltInCodeGenMacros.DisplayShorthand;
					}
					else
					{
						macro = BuiltInCodeGenMacros.Display;
					}

					if (macro != null)
					{
						macro(this, macroName,
							match.Groups["macroArg"].Success ? match.Groups["macroArg"].Value : null
						);
					}
				}

				// Link
				else if (match.Groups["link"].Success)
				{
					if (!NoOutput)
					{
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
							string.Format("() =>{{ {0}; return null; }}", BuildExpression(match.Groups["linkSetter"].Value)) : // stick the setter into a lambda
							null;

						Code.Indent();
						Code.Buffer
							.AppendFormat("yield return link(@\"{0}\", {1}, {2});",
								text.Replace("\"", "\"\""),
								linkTarget.IndexOf('(') >= 1 ? linkTarget : string.Format("@\"{0}\"", linkTarget.Replace("\"", "\"\"")), // if a peren is present, treat as a function
								setters == null ? "null" : setters
							)
							.AppendLine();
					}
				}

				// Naked var
				else if (match.Groups["nakedVar"].Success)
				{
					if (!NoOutput)
					{
						Code.Indent();
						Code.Buffer
							.AppendFormat("yield return text({0});", BuildVariableRef(match.Groups["nakedVar"].Value))
							.AppendLine();
					}
				}
			};

			Code.Indent();
			Code.Buffer.Append("yield break;");
		}

		string BuildVariableRef(string varName)
		{
			Importer.RegisterVar(varName);
			return string.Format("Vars.{0}", EscapeReservedWord(varName));
		}

		//string CleanupArrays(string expression)
		//{
		//	return rx_Array.Replace(expression, array =>
		//	{
		//		return string.Format("array({0})", CleanupArrays(array.Groups["args"].Value));
		//	});
		//}

		public string BuildExpression(string expression)
		{
			string clean = expression;

			clean = rx_Vars.Replace(clean, varName =>
			{
				return BuildVariableRef(varName.Groups[1].Value);
			});

			clean = rx_Operator.Replace(clean, op =>
			{
				switch (op.Value)
				{
					case "and": return "&&";
					case "or": return "||";
					case "eq":
					case "is not": return "!=";
					case "is": return "==";
					case "lt": return "<";
					case "lte": return "<=";
					case "gt": return ">";
					case "gte": return ">=";
					case "to": return "=";
					case "not": return "!";
				};
				return string.Empty;
			});

			return clean;
		}

		public static string ParamsWithCommas(string rawParams)
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
	}

	[Serializable]
	public class SugarStoryData
	{
		public string startPid;
		public PassageData[] passages;
	}
}