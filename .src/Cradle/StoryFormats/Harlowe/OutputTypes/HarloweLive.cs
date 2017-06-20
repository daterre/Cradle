using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using IStoryThread = System.Collections.Generic.IEnumerable<Cradle.StoryOutput>;

namespace Cradle.StoryFormats.Harlowe
{
	public class HarloweLive : EmbedFragment
	{
		public float seconds;

		public HarloweLive(float seconds, Func<IStoryThread> fragment)
			: base(fragment)
		{
		}
	}

	public class HarloweLiveStop: StoryOutput
	{
	}
}

