using UnityEngine;
using UnityTwine;
using UnityTest;
using System.Collections;
using System.Linq;
using UnityTwine;
using UnityTwine.StoryFormats.Harlowe;

public class UnityTwineTestMacros: TwineRuntimeMacros
{
	[TwineRuntimeMacro]
	public void assert(bool condition, string error)
	{
		IntegrationTest.Assert(condition, error);
	}

	[TwineRuntimeMacro]
	public void assertStyle(string setting, TwineVar value)
	{
		IntegrationTest.Assert(
			value == this.Story.Style.GetValues(setting).LastOrDefault(),
			string.Format("Style's \"{0}\" setting is not {1}", setting, value)
		);
		IntegrationTest.Assert(
			this.Story.Output.Reverse<TwineOutput>().Any(output => value == output.Style.GetValues(setting).LastOrDefault()),
			string.Format("Style's \"{0}\" setting was not applied to any output", setting)
		);
	}

	[TwineRuntimeMacro]
	public void assertHook(string hookName)
	{
		IntegrationTest.Assert(
			this.Story.Style.GetValues<HarloweHook>(HarloweStyleSettings.Hook).Any(val => val.HookName == hookName),
			string.Format("Can't find a Harlowe-hook named {0} in the current style.", hookName)
		);
		IntegrationTest.Assert(
			this.Story.Output.Reverse<TwineOutput>().Any(output => output.Style.GetValues<HarloweHook>(HarloweStyleSettings.Hook).Any(val => val.HookName == hookName)),
			string.Format("A Harlowe-hook named {0} wasn't applied to any output, despite being in scope.", hookName)
		);
	}

	[TwineRuntimeMacro]
	public void assertText(string text, string error)
	{
		IntegrationTest.Assert(this.Story.Text.Where(t => t.Text == text).Count() == 1, error);
	}

	[TwineRuntimeMacro]
	public void assertLink(string text, string passage, string error = null)
	{
		IntegrationTest.Assert(this.Story.Links.Where(link => link.Text == text && link.PassageName == passage).Count() == 1, error ?? text);
	}

	[TwineRuntimeMacro]
	public void assertNoOutput(System.Type type )
	{
		IntegrationTest.Assert(this.Story.Output.Where(output => type.IsAssignableFrom(output.GetType())).Count() == 0);
	}

	[TwineRuntimeMacro]
	public void pass()
	{
		IntegrationTest.Pass();
	}
}
