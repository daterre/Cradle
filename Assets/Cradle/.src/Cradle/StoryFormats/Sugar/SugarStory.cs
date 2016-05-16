using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IStoryThread = System.Collections.Generic.IEnumerable<Cradle.StoryOutput>;

namespace Cradle.StoryFormats.Sugar
{
	public abstract class SugarStory: Story
	{
		protected StoryVar array(params StoryVar[] vars)
		{
			return new StoryVar(vars);
		}

		protected StoryVar either(params StoryVar[] vars)
		{
			return vars[UnityEngine.Random.Range(0, vars.Length)];
		}

		protected int random(int max)
		{
			return random(0, max);
		}

		protected int random(int min, int max)
		{
			return UnityEngine.Random.Range(min, max + 1);
		}

		protected double randomFloat(double max)
		{
			return randomFloat(0, max);
		}

		protected double randomFloat(double min, double max)
		{
			return UnityEngine.Random.Range((float)min, (float)max);
		}

		protected string passage()
		{
			return this.CurrentPassageName;
		}

		protected string previous()
		{
			return this.PassageHistory.LastOrDefault();
		}

		protected StoryVar visited(params string[] passageNames)
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

		protected StoryVar visitedTag(params string[] tags)
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

		protected int time()
		{
			return UnityEngine.Mathf.RoundToInt(this.PassageTime * 1000);
		}

		protected StoryVar parameter(int index)
		{
			for (int i = this.Output.Count-1; i >= 0 ; i--)
			{
				EmbedPassage embed = this.Output[i] is EmbedPassage ?
					(EmbedPassage)this.Output[i] :
					this.Output[i].EmbedInfo as EmbedPassage;

				if (embed != null)
				{
					if(embed.Parameters == null || embed.Parameters.Length - 1 < index)
						break;
					else
						return embed.Parameters[index];
				}	 		
			}

			return new StoryVar(index);
		}
	}
}
