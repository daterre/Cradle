using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IStoryThread = System.Collections.Generic.IEnumerable<Cradle.StoryOutput>;

namespace Cradle.StoryFormats.Sugar
{
	public abstract class SugarStory: Story
	{
		public SugarStory()
		{
			StoryVar.RegisterTypeService<List<StoryVar>>(new SugarListService());
			StoryVar.RegisterTypeService<Dictionary<string, StoryVar>>(new SugarDictionaryService());
		}

		protected override Func<IStoryThread> GetPassageThread(StoryPassage passage)
		{
			return () => GetPassageThreadWithHeaderAndFooter(passage.MainThread, this.PassageHistory.Count == 0);
		}

		IStoryThread GetPassageThreadWithHeaderAndFooter(Func<IStoryThread> mainThread, bool startup)
		{
			if (startup && this.Passages.ContainsKey("StoryInit"))
				yield return passage("StoryInit");

			if (this.Passages.ContainsKey("PassageHeader"))
				yield return passage("PassageHeader");

			yield return fragment(mainThread);

			if (this.Passages.ContainsKey("PassageFooter"))
				yield return passage("PassageFooter");
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
			return this.CurrentPassage.Name;
		}

		protected string previous()
		{
			return this.PassageHistory.Count < 1 ? null : this.PassageHistory[this.PassageHistory.Count-1];
		}

		protected StoryVar visited(params string[] passageNames)
		{
			if (passageNames == null || passageNames.Length == 0)
				passageNames = new string[] { this.CurrentPassage.Name };

			int min = int.MaxValue;
			for (int i = 0; i < passageNames.Length; i++)
			{
				string passage = passageNames[i];
                int count = PassageHistory.Where(p => p == passage).Count();
				if (passage == this.CurrentPassage.Name)
					count++;
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
				if (CurrentPassage.Tags.Contains(tag))
					count++;
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
			return this.CurrentPassage.Tags;
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

		// ................................
		// Javascript alternatives

		protected StoryVar array(params StoryVar[] vars)
		{
			return new StoryVar(new List<StoryVar>(vars));
		}

		protected StoryVar obj(params StoryVar[] vals)
		{
			if (vals.Length % 2 != 0)
				throw new VarTypeException("To create an object map you must pass an even number of parameters.");

			var dictionary = new Dictionary<string, StoryVar>();

			for (int i = 0; i < vals.Length; i+=2)
			{
				string key;
				if (!StoryVar.TryConvertTo<string>(vals[i], out key))
					throw new VarTypeException("To create an object map, every odd parameter (an entry name) must be a string.");

				dictionary[key] = vals[i + 1];
			}

			return new StoryVar(dictionary);
		}
	}
}
