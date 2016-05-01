using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ITwineThread = System.Collections.Generic.IEnumerable<UnityTwine.TwineOutput>;

namespace UnityTwine.StoryFormats.Harlowe
{
	public class Live : TwineEmbedFragment
	{
		public float Milliseconds;

		public Live(float ms, Func<ITwineThread> fragment)
			: base(fragment)
		{
		}
	}

	public class LiveStop: TwineOutput
	{
	}
}

