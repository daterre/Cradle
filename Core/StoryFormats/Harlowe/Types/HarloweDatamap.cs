using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace UnityTwine.StoryFormats.Harlowe
{
	public class HarloweDatamap : HarloweCollection
	{
		internal Dictionary<string,TwineVar> Dictionary;

		public HarloweDatamap()
		{
			Dictionary = new Dictionary<string,TwineVar>();
		}

		public HarloweDatamap(IDictionary<string,TwineVar> map)
		{
			Dictionary = new Dictionary<string, TwineVar>(map);
		}

		public HarloweDatamap(params TwineVar[] vals):this()
		{
			if (vals.Length % 2 != 0)
				throw new TwineTypeException("To create a datamap you must pass an even number of parameters.");
			for (int i = 0; i < vals.Length; i+=2)
			{
				string key;
				if (!TwineVar.TryConvertTo<string>(vals[i], out key))
					throw new TwineTypeException("To create a datamap, every odd parameter (an entry name) must be string.");

				Dictionary[key] = vals[i + 1];
			}
		}

		public int Length
		{
			get { return Dictionary.Count; }
		}

		public bool ContainsKey(string key)
		{
			return Dictionary.ContainsKey(key);
		}

		public override IEnumerable<TwineVar> Flatten()
		{
			foreach(TwineVar val in Dictionary.Values)
				yield return val.Clone();
		}

		public override ITwineType Clone()
		{
			var clone = new HarloweDatamap();
			foreach (var pair in this.Dictionary)
				clone.Dictionary[pair.Key] = pair.Value.Clone();

			return clone;
		}

		public override string ToString()
		{
			var str = new StringBuilder();
			foreach(var pair in Dictionary.OrderBy(v => v.Key))
			{
				if (str.Length > 0)
					str.Append(',');
				str.AppendFormat("{0}: {1}", pair.Key, pair.Value);
			}

			return str.ToString();
		}

		public override TwineVar GetMember(string memberName)
		{
			TwineVar val;
			
			if (!Dictionary.TryGetValue(memberName, out val))
				throw new TwineTypeMemberException(string.Format("The datamap doesn't have an entry under the name '{0}'.", memberName));

			return new TwineVar(val);
		}

		public override void SetMember(string memberName, TwineVar value)
		{
			Dictionary[memberName] = value.Clone();
		}

		public override void RemoveMember(string memberName)
		{
			Dictionary.Remove(memberName);
		}

		public override bool Compare(TwineOperator op, object b, out bool result)
		{
			result = false;

			switch (op)
			{
				case TwineOperator.Equals: {
					if (!(b is HarloweDatamap))
						return false;

					var bMap = (HarloweDatamap)b;
					result =
						new HashSet<string>(Dictionary.Keys).SetEquals(bMap.Dictionary.Keys) &&
						new HashSet<TwineVar>(Dictionary.Values).SetEquals(bMap.Dictionary.Values);
					break;
				}
				case TwineOperator.Contains: {
					result = this.ContainsKey(TwineVar.ConvertTo<string>(b));
					break;
				}
			}

			return true;
		}

		public override bool Combine(TwineOperator op, object b, out TwineVar result)
		{
			result = default(TwineVar);

			switch (op)
			{
				case TwineOperator.Add: {
					if (!(b is HarloweDatamap))
						return false;
					var bMap = (HarloweDatamap)b;
					var output = new Dictionary<string, TwineVar>(this.Dictionary);
					bMap.Dictionary.ToList().ForEach(x => output[x.Key] = x.Value);
					result = new HarloweDatamap(output);
					break;
				}
				case TwineOperator.Subtract: { 
					// Going out on a limb here, adding subtraction support to Harlowe:
					// Treat any set as keys to remove from the dictionary

					IEnumerable<string> keys;
					if (b is HarloweDatamap)
						keys = ((HarloweDatamap)b).Dictionary.Keys;
					else if (b is HarloweDataset)
						keys = ((HarloweDataset)b).Values.Select(v => v.ToString());
					else if (b is HarloweArray)
						keys = ((HarloweArray)b).Values.Select(v => v.ToString());
					else
						return false;
					
					var keySet = new HashSet<string>(keys);

					// A little linq-fu
					result = new HarloweDatamap(this.Dictionary
						.Where(pair => !keySet.Contains(pair.Key))
						.ToDictionary(
							pair => pair.Key,
							pair => pair.Value,
							System.StringComparer.OrdinalIgnoreCase
						)
					);
					break;
				}
				default:
					return false;
			}

			return true;
		}

		public override bool Unary(TwineOperator op, out TwineVar result)
		{
			result = default(TwineVar);
			return false;
		}

		public override bool ConvertTo(System.Type t, out object result, bool strict = false)
		{
			result = null;

			if (!strict && t == typeof(string))
			{
				result = this.ToString();
				return true;
			}
			else
				return false;
		}
	}
}