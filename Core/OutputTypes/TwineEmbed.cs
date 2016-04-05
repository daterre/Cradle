using System;
using System.Collections;
using System.Collections.Generic;

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
		public TwineEmbedFragment(string fragmentName)
		{
			this.Name = fragmentName;
		}
	}

	public class TwineEmbedOpen: TwineOutput
	{
		public TwineEmbed EmbedInfo;
	}

	public class TwineEmbedClose: TwineOutput
	{
		public TwineEmbedOpen Opener;
	}
}