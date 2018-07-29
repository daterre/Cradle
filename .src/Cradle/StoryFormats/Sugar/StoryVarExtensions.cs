using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cradle;

namespace Cradle.StoryFormats.Sugar
{
	public static class SugarVarTypeExtensions
	{
		static void AssertSugarList(StoryVar arr)
		{
			if (arr.InnerType != typeof(List<StoryVar>))
				throw new InvalidOperationException("Can't do this, the variable is not an array");
		}

		static void AssertSugarDict(StoryVar arr)
		{
			if (arr.InnerType != typeof(Dictionary<string, StoryVar>))
				throw new InvalidOperationException("Can't do this, the variable is not an object map");
		}

		// -------------------

		public static StoryVar indexOf(this StoryVar arr, StoryVar value)
		{
			AssertSugarList(arr);
			return arr.ConvertValueTo<List<StoryVar>>().IndexOf(value);
		}

		public static StoryVar lastIndexOf(this StoryVar arr, StoryVar value)
		{
			AssertSugarList(arr);
			return arr.ConvertValueTo<List<StoryVar>>().LastIndexOf(value);
		}

		public static StoryVar includes(this StoryVar arr, StoryVar value, int position = 0)
		{
			AssertSugarList(arr);
			if (position > 0)
				return arr.ConvertValueTo<List<StoryVar>>().Skip(position).Contains(value);
			else
				return arr.ConvertValueTo<List<StoryVar>>().Contains(value);
		}

		public static StoryVar hasOwnProperty(this StoryVar obj, StoryVar value)
		{
			AssertSugarDict(obj);
			return obj.ConvertValueTo<Dictionary<string, StoryVar>>().ContainsKey(value);
		}

		public static StoryVar includesAll(this StoryVar arr, params StoryVar[] values)
		{
			AssertSugarList(arr);
			var curArr = arr.ConvertValueTo<List<StoryVar>>();
			return values.All(val => curArr.Contains(val));
		}

		public static StoryVar includesAll(this StoryVar arr, StoryVar value)
		{
			AssertSugarList(arr);
			var valArr = value.InnerValue as List<StoryVar>;
			if (valArr != null)
				return arr.includesAll(valArr.ToArray());
			else
				return arr.includesAll(value);
		}

		public static StoryVar includesAny(this StoryVar arr, params StoryVar[] values)
		{
			AssertSugarList(arr);
			var curArr = arr.ConvertValueTo<List<StoryVar>>();
			return values.Any(val => curArr.Contains(val));
		}

		public static StoryVar includesAny(this StoryVar arr, StoryVar value)
		{
			AssertSugarList(arr);
			var valArr = value.InnerValue as List<StoryVar>;
			if (valArr != null)
				return arr.includesAny(valArr.ToArray());
			else
				return arr.includesAny(value);
		}

		#region Depcrecated
		public static StoryVar contains(this StoryVar arr, StoryVar value, int position = 0) { return arr.includes(value, position); }
		public static StoryVar containsAll(this StoryVar arr, params StoryVar[] values) { return arr.includesAll(values); }
		public static StoryVar containsAll(this StoryVar arr, StoryVar value) { return arr.includesAll(value); }
		public static StoryVar containsAny(this StoryVar arr, params StoryVar[] values) { return arr.containsAny(values); }
		public static StoryVar containsAny(this StoryVar arr, StoryVar value) { return arr.contains(value); }
		#endregion

		public static StoryVar length(this StoryVar arr)
		{
			AssertSugarList(arr);
			return arr.ConvertValueTo<List<StoryVar>>().Count;
		}

		public static StoryVar count(this StoryVar arr, StoryVar value, int position = 0)
		{
			AssertSugarList(arr);
			return arr.ConvertValueTo<List<StoryVar>>().Skip(position).Count(v => v == value);
		}

		public static StoryVar push(this StoryVar arr, params StoryVar[] values)
		{
			AssertSugarList(arr);
			var list = arr.InnerValue as List<StoryVar>;
			list.AddRange(values);
			return list.Count;
		}

		public static StoryVar pushUnique(this StoryVar arr, params StoryVar[] values)
		{
			AssertSugarList(arr);
			var list = arr.InnerValue as List<StoryVar>;
			for (int i = 0; i < values.Length; i++)
				if (!list.Contains(values[i]))
					list.Add(values[i]);
			return list.Count;
		}

		public static StoryVar pop(this StoryVar arr)
		{
			AssertSugarList(arr);
			var list = arr.InnerValue as List<StoryVar>;
			StoryVar last = list.LastOrDefault();
			list.RemoveAt(list.Count - 1);
			return last;
		}

		public static StoryVar shift(this StoryVar arr)
		{
			AssertSugarList(arr);
			var list = arr.InnerValue as List<StoryVar>;
			StoryVar first = list.FirstOrDefault();
			list.RemoveAt(0);
			return first;
		}

		public static StoryVar unshift(this StoryVar arr, params StoryVar[] values)
		{
			AssertSugarList(arr);
			var list = arr.InnerValue as List<StoryVar>;
			list.InsertRange(0, values);
			return list.Count;
		}

		public static StoryVar unshiftUnique(this StoryVar arr, params StoryVar[] values)
		{
			AssertSugarList(arr);
			var list = arr.InnerValue as List<StoryVar>;
			for (int i = 0; i < values.Length; i++)
				if (!list.Contains(values[i]))
					list.Insert(values[i], 0);
			return list.Count;
		}

		public static StoryVar splice(this StoryVar arr, int index, int count, params StoryVar[] values)
		{
			AssertSugarList(arr);
			var list = arr.ConvertValueTo<List<StoryVar>>();
			List<StoryVar> removed = null;
			if (count > 0)
			{
				removed = list.GetRange(index, count);
				list.RemoveRange(index, count);
			}
			list.InsertRange(index, values);
			return new StoryVar(removed);
		}

		public static StoryVar slice(this StoryVar arr, int start, int end)
		{
			AssertSugarList(arr);
			var list = arr.ConvertValueTo<List<StoryVar>>();
			start = start >= 0 ? start : list.Count + start;
			end = end >= 0 ? end : list.Count + end;
			int count = end - start;

			return new StoryVar(list.GetRange(start, count));
		}

		public static StoryVar concat(this StoryVar arr, StoryVar otherArr)
		{
			var a = arr.ConvertValueTo<List<StoryVar>>();
			var b = otherArr.ConvertValueTo<List<StoryVar>>();

			return new StoryVar(a.Concat(b).ToList());
		}

		public static StoryVar concatUnique(this StoryVar arr, StoryVar otherArr)
		{
			var a = new List<StoryVar>();
			var b = new List<StoryVar>();
			DoFlatten(ref a, arr);
			DoFlatten(ref b, otherArr);

			return new StoryVar(a.Union(b).ToList());
		}

		public static StoryVar delete(this StoryVar obj, StoryVar key)
		{
			if (obj.InnerType == typeof(Dictionary<string, StoryVar>))
			{
				return obj.ConvertValueTo<Dictionary<string, StoryVar>>().Remove(key);
			}
			else
				return delete(obj, new StoryVar[] { key }); // Use array implementation
		}

		public static StoryVar delete(this StoryVar arr, params StoryVar[] values)
		{
			AssertSugarList(arr);
			var removed = new List<StoryVar>();
			arr.ConvertValueTo<List<StoryVar>>().RemoveAll(v =>
			{
				bool remove = values.Contains(v);
				if (remove)
					removed.Add(v);
				return remove;
			});

			return new StoryVar(removed);
		}

		public static StoryVar deleteAt(this StoryVar arr, params int[] indices)
		{
			AssertSugarList(arr);
			var curArr = arr.ConvertValueTo<List<StoryVar>>();
			var removed = new List<StoryVar>();
			foreach (int index in indices.OrderByDescending(i => i))
			{
				removed.Add(curArr[index]);
				curArr.RemoveAt(index);
			}

			return new StoryVar(removed);
		}

		public static StoryVar pluck(this StoryVar arr)
		{
			AssertSugarList(arr);
			return arr.deleteAt(UnityEngine.Random.Range(0, arr.length()));
		}

		public static StoryVar pluckMany(this StoryVar arr, int want = 0)
		{
			AssertSugarList(arr);
			if (want <= 0)
				want = UnityEngine.Random.Range(0, arr.length());

			int[] indices = new int[UnityEngine.Mathf.Max(want, arr.length())];

			for (int i = 0; i < indices.Length; i++)
			{
				int test;
				do { test = UnityEngine.Random.Range(0, arr.length()); }
				while (indices.Contains(test));

				indices[i] = test;
			}

			return arr.deleteAt(indices);
		}

		public static StoryVar random(this StoryVar arr)
		{
			AssertSugarList(arr);
			var list = arr.ConvertValueTo<List<StoryVar>>();
			return list[UnityEngine.Random.Range(0, arr.length())];
		}

		public static StoryVar randomMany(this StoryVar arr, int want = 0)
		{
			AssertSugarList(arr);
			if (want <= 0)
				want = UnityEngine.Random.Range(0, arr.length());

			var list = arr.ConvertValueTo<List<StoryVar>>();
			var newList = new List<StoryVar>();
			int count = UnityEngine.Mathf.Max(want, list.Count);

			while (newList.Count < count)
			{
				StoryVar test;
				do { test = UnityEngine.Random.Range(0, arr.length()); }
				while (newList.Contains(test));

				newList.Add(test);
			}

			return new StoryVar(newList);
		}

		public static StoryVar flatten(this StoryVar arr)
		{
			AssertSugarList(arr);

			var list = new List<StoryVar>();
			DoFlatten(ref list, arr);
			return new StoryVar(list);
		}

		static void DoFlatten(ref List<StoryVar> list, StoryVar value)
		{
			var innerList = value.InnerValue as List<StoryVar>;
			if (innerList == null)
				list.Add(value);
			else
				for (int i = 0; i < innerList.Count; i++)
					DoFlatten(ref list, innerList[i]);
		}

		public static void sort(this StoryVar arr)
		{
			AssertSugarList(arr);
			var list = arr.InnerValue as List<StoryVar>;
			list.Sort((a, b) => String.Compare(a.ToString(), b.ToString()));
		}

		public static void reverse(this StoryVar arr)
		{
			AssertSugarList(arr);
			var list = arr.InnerValue as List<StoryVar>;
			list.Reverse();
		}

		public static void shuffle(this StoryVar arr)
		{
			AssertSugarList(arr);
			var list = arr.InnerValue as List<StoryVar>;
			list.Sort((a, b) => UnityEngine.Random.Range(-1, 2));
		}

		public static StoryVar toString(this StoryVar value)
		{
			return value.ToString();
		}
	}
}
