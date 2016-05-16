using System;
using System.Collections;
using System.Collections.Generic;
using IStoryThread = System.Collections.Generic.IEnumerable<Cradle.StoryOutput>;

namespace Cradle
{
	public abstract class Embed: StoryOutput
	{
	}

	public class EmbedPassage : Embed
    {
		public StoryVar[] Parameters;

        public EmbedPassage(string passageName, params StoryVar[] parameters)
        {
            this.Name = passageName;
			this.Parameters = parameters;
        }
    }

	public class EmbedFragment : Embed
	{
        public Func<IStoryThread> GetThread;

        public EmbedFragment(Func<IStoryThread> fragment)
		{
			GetThread = fragment;
		}
	}
}