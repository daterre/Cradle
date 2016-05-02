using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ITwineThread = System.Collections.Generic.IEnumerable<UnityTwine.TwineOutput>;

namespace UnityTwine.StoryFormats.Harlowe
{
	public class HarloweLive : TwineEmbedFragment
	{
		public float Milliseconds;

		public HarloweLive(float ms, Func<ITwineThread> fragment)
			: base(fragment)
		{
		}
	}

	public class HarloweLiveStop: TwineOutput
	{
	}
}

