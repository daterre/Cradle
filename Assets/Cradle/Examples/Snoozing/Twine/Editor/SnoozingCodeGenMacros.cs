using UnityEngine;
using UnityEditor;
using Cradle.Editor.StoryFormats.Sugar;

[InitializeOnLoad]
public static class SnoozingCodeGenMacros
{
	static SnoozingCodeGenMacros()
	{
		SugarTranscoder.CodeGenMacros["waitforclick"] = WaitForClick;
		SugarTranscoder.CodeGenMacros["wait"] = WaitForSeconds;
	}

	public static SugarCodeGenMacro WaitForClick = (transcoder, macro, argument) =>
	{
		transcoder.Code.Indent();
		transcoder.Code.Buffer
			.AppendLine("SendMessage(\"WaitForClick\"); yield return null;");
	};

	public static SugarCodeGenMacro WaitForSeconds = (transcoder, macro, argument) =>
	{
		transcoder.Code.Indent();
		transcoder.Code.Buffer
			.AppendFormat("SendMessage(\"WaitForSeconds\", (float) ({0})); yield return null;", transcoder.BuildExpression(argument))
			.AppendLine();
	};
}