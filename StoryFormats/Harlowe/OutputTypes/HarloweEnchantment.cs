using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ITwineThread = System.Collections.Generic.IEnumerable<UnityTwine.TwineOutput>;

namespace UnityTwine.StoryFormats.Harlowe
{
	public class Enchantment: TwineEmbedFragment
	{
        public EnchantmentTarget[] Targets;
		public Func<ITwineThread> Action;

        public Enchantment(TwineStory story, EnchantmentTarget[] targets, Func<ITwineThread> action): base(action)
		{
            this.Targets = targets;
            this.Action = () => EnchantTargets(story, action);
		}

        ITwineThread EnchantTargets(TwineStory story, Func<ITwineThread> action)
        {
            using (story.Context.Apply(HarloweContextOptions.EnchantSource, this))
            {  
                foreach (TwineOutput output in action())
                    yield return output;  
            }
        }
	}

    public class EnchantmentTarget
    {
        public TwineOutput Output;
        public Regex Occurences;
    }
}

