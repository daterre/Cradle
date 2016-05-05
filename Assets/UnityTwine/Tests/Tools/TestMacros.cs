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
	public void assertContext(string option, TwineVar value)
	{
		IntegrationTest.Assert(
			value == this.Story.Context.GetValues(option).LastOrDefault(),
			string.Format("Context option {0} is not {1}", option, value)
		);
		IntegrationTest.Assert(
			this.Story.Output.Reverse<TwineOutput>().Any(output => value == output.ContextInfo.GetValues(option).LastOrDefault()),
			string.Format("Context option {0} was not applied to any output", option)
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
