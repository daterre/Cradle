using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITwineThread = System.Collections.Generic.IEnumerable<UnityTwine.TwineOutput>;

namespace UnityTwine.StoryFormats.Sugar
{
	public abstract class SugarStory: TwineStory
	{
		protected TwineVar array(params TwineVar[] vars)
		{
			return new TwineVar(vars);
		}

		protected TwineVar either(params TwineVar[] vars)
		{
			return vars[UnityEngine.Random.Range(0, vars.Length)];
		}

		protected int random(int min, int max)
		{
			return UnityEngine.Random.Range(min, max + 1);
		}

		protected string passage()
		{
			return this.CurrentPassageName;
		}

		protected string previous()
		{
			return this.PreviousPassageName;
		}

		protected TwineVar visited(params string[] passageNames)
		{
			if (passageNames == null || passageNames.Length == 0)
				passageNames = new string[] { this.CurrentPassageName };

			int min = int.MaxValue;
			for (int i = 0; i < passageNames.Length; i++)
			{
				string passage = passageNames[i];
                int count = PassageHistory.Where(p => p == passage).Count();
				if (count < min)
					min = count;
			}

			if (min == int.MaxValue)
				min = 0;

			return min;
		}

		protected TwineVar visitedTag(params string[] tags)
		{
			if (tags == null || tags.Length == 0)
				return 0;

			int min = int.MaxValue;
			for (int i = 0; i < tags.Length; i++)
			{
				string tag = tags[i];
                int count = PassageHistory.Where(p => Passages[p].Tags.Contains(tag)).Count();
				if (count < min)
					min = count;
			}

			if (min == int.MaxValue)
				min = 0;

			return min;
		}

		protected int turns()
		{
			return NumberOfLinksDone;
		}

		protected string[] tags()
		{
			return this.Tags;
		}

		protected TwineVar parameter(int index)
		{
			for (int i = this.Output.Count-1; i >= 0 ; i--)
			{
				TwineEmbedPassage embed = this.Output[i] is TwineEmbedPassage ?
					(TwineEmbedPassage)this.Output[i] :
					this.Output[i].EmbedInfo as TwineEmbedPassage;

				if (embed != null)
				{
					if(embed.Parameters == null || embed.Parameters.Length - 1 < index)
						break;
					else
						return embed.Parameters[index];
				}	 		
			}

			return new TwineVar(index);
		}
	}
}
