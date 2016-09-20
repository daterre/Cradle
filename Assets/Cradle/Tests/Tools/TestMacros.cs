using UnityEngine;
using Cradle;
using UnityTest;
using System.Collections;
using System.Linq;
using Cradle;
using Cradle.StoryFormats.Harlowe;

public class UnityTwineTestMacros: RuntimeMacros
{
	[RuntimeMacro]
	public void assert(bool condition, string error)
	{
		IntegrationTest.Assert(condition, error);
	}

	[RuntimeMacro]
	public void assertStyle(string setting, StoryVar value)
	{
		StoryStyle style = this.Story.GetCurrentStyle();
		IntegrationTest.Assert(
			value == style[setting],
			string.Format("Group's \"{0}\" style setting is not {1}", setting, value)
		);
		IntegrationTest.Assert(
			this.Story.Output.Reverse<StoryOutput>().Any(output => value == output.GetAppliedStyle()[setting]),
			string.Format("The \"{0}\" style setting was not applied to any output", setting)
		);
	}

	[RuntimeMacro]
	public void assertHook(string hookName)
	{
		IntegrationTest.Assert(
			this.Story.CurrentGroup.Style.Get<string>(HarloweStyleSettings.Hook) == hookName,
			string.Format("The current group isn't defined as a Harlowe-hook named'{0}'.", hookName)
		);
		IntegrationTest.Assert(
			this.Story.Output.Reverse<StoryOutput>().Any(output => output.GetAppliedStyle().Get<string>(HarloweStyleSettings.Hook) == hookName),
			string.Format("The Harlowe-hook '{0}' wasn't applied to any output, despite being in scope.", hookName)
		);
	}

	[RuntimeMacro]
	public void assertText(string text, string error)
	{
		IntegrationTest.Assert(this.Story.GetCurrentText().Where(t => t.Text == text).Count() == 1, error);
	}

	[RuntimeMacro]
	public void assertLink(string text, string passage, string error = null)
	{
		IntegrationTest.Assert(this.Story.GetCurrentLinks().Where(link => link.Text == text && link.PassageName == passage).Count() == 1, error ?? text);
	}

	[RuntimeMacro]
	public void assertNoOutput(System.Type type )
	{
		IntegrationTest.Assert(this.Story.Output.Where(output => type.IsAssignableFrom(output.GetType())).Count() == 0);
	}

	[RuntimeMacro]
	public void pass()
	{
		IntegrationTest.Pass();
	}
}
