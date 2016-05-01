using System;
using System.Collections;
using System.Collections.Generic;
using ITwineThread = System.Collections.Generic.IEnumerable<UnityTwine.TwineOutput>;

namespace UnityTwine
{
	public abstract class TwineEmbed: TwineOutput
	{
	}

	public class TwineEmbedPassage : TwineEmbed
    {
		public TwineVar[] Parameters;

        public TwineEmbedPassage(string passageName, params TwineVar[] parameters)
        {
            this.Name = passageName;
			this.Parameters = parameters;
        }
    }

	public class TwineEmbedFragment : TwineEmbed
	{
        public Func<ITwineThread> GetThread;

        public TwineEmbedFragment(Func<ITwineThread> fragment)
		{
			GetThread = fragment;
		}
	}
}