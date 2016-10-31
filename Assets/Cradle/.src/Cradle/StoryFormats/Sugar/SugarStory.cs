using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IStoryThread = System.Collections.Generic.IEnumerable<Cradle.StoryOutput>;

namespace Cradle.StoryFormats.Sugar
{
	public abstract class SugarStory: Story
	{
		protected override Func<IStoryThread> GetPassageThread(StoryPassage passage)
		{
			return () => GetPassageThreadWithHeaderAndFooter(passage.MainThread, this.PassageHistory.Count == 1);
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
			return this.CurrentPassageName;
		}

		protected string previous()
		{
			return this.PassageHistory.Count < 2 ? null : this.PassageHistory[this.PassageHistory.Count-2];
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

		// ................................
		// Arrays

		protected StoryVar array(params StoryVar[] vars)
		{
			return new StoryVar(new List<StoryVar>(vars));
		}

		protected StoryVar arrayGet(StoryVar arr, int index)
		{
			return arr.ConvertValueTo<List<StoryVar>>()[index];
		}

		protected StoryVar arraySet(StoryVar arr, int index, StoryVar value)
		{
			return arr.ConvertValueTo<List<StoryVar>>()[index] = value;
		}

		protected int arrayLength(StoryVar arr)
		{
			return arr.ConvertValueTo<List<StoryVar>>().Count;
		}

		protected int arrayIndexOf(StoryVar arr, StoryVar value)
		{
			return arr.ConvertValueTo<List<StoryVar>>().IndexOf(value);
		}

		protected void arrayAdd(StoryVar arr, StoryVar value)
		{
			arr.ConvertValueTo<List<StoryVar>>().Add(value);
		}

		protected void arrayInsert(StoryVar arr, int index, StoryVar value)
		{
			arr.ConvertValueTo<List<StoryVar>>().Insert(index, value);
		}

		protected StoryVar arrayDelete(StoryVar arr, params StoryVar[] values)
		{
			var newArr = new List<StoryVar>();
			arr.ConvertValueTo<List<StoryVar>>().RemoveAll(v => {
				bool remove = values.Contains(v);
				if (remove)
					newArr.Add(v);
				return remove;
			});

			return new StoryVar(newArr);
		}

		protected StoryVar arrayDeleteAt(StoryVar arr, params int[] indices)
		{
			var curArr = arr.ConvertValueTo<List<StoryVar>>();
			var newArr = new List<StoryVar>();
			foreach (int index in indices.OrderByDescending(i => i))
			{
				newArr.Add(curArr[index]);
				curArr.RemoveAt(index);
			}

			return new StoryVar(newArr);
		}

		protected bool arrayContains(StoryVar arr, StoryVar value, int position = 0)
		{
			if (position > 0)
				return arr.ConvertValueTo<List<StoryVar>>().Skip(position).Contains(value);
			else
				return arr.ConvertValueTo<List<StoryVar>>().Contains(value);
		}

		protected bool arrayContainsAll(StoryVar arr, params StoryVar[] values)
		{
			var curArr = arr.ConvertValueTo<List<StoryVar>>();
			return values.All(val => curArr.Contains(val));
		}

		protected bool arrayContainsAll(StoryVar arr, StoryVar valArray)
		{
			var valArr = valArray.InnerValue as List<StoryVar>;
			if (valArr != null)
				return arrayContainsAll(arr, valArr.ToArray());
			else
				return arrayContains(arr, valArray);
		}

		protected bool arrayContainsAny(StoryVar arr, params StoryVar[] values)
		{
			var curArr = arr.ConvertValueTo<List<StoryVar>>();
			return values.Any(val => curArr.Contains(val));
		}

		protected int arrayCount(StoryVar arr, StoryVar value, int position = 0)
		{
			return arr.ConvertValueTo<List<StoryVar>>().Skip(position).Count(v => v == value);
		}

		// ................................
		// Objects

		protected StoryVar obj(params StoryVar[] vals)
		{
			if (vals.Length % 2 != 0)
				throw new VarTypeException("To create an object you must pass an even number of parameters.");

			var dictionary = new Dictionary<string, StoryVar>();

			for (int i = 0; i < vals.Length; i+=2)
			{
				string key;
				if (!StoryVar.TryConvertTo<string>(vals[i], out key))
					throw new VarTypeException("To create an object, every odd parameter (an entry name) must be a string.");

				dictionary[key] = vals[i + 1];
			}

			return new StoryVar(dictionary);
		}

		protected StoryVar objGet(StoryVar obj, string key)
		{
			StoryVar output = default(StoryVar);
			obj.ConvertValueTo<Dictionary<string, StoryVar>>().TryGetValue(key, out output);
			return output;
		}

		protected void objSet(StoryVar obj, string key, StoryVar value)
		{
			obj.ConvertValueTo<Dictionary<string, StoryVar>>()[key] = value;
		}

		protected void objDelete(StoryVar obj, string key)
		{
			obj.ConvertValueTo<Dictionary<string, StoryVar>>().Remove(key);
		}

		protected int objLength(StoryVar obj)
		{
			return obj.ConvertValueTo<Dictionary<string, StoryVar>>().Count;
		}

		protected bool objContains(StoryVar obj, string key)
		{
			return obj.ConvertValueTo<Dictionary<string, StoryVar>>().ContainsKey(key);
		}
	}
}
