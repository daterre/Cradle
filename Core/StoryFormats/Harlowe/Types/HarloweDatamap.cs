using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace UnityTwine.StoryFormats.Harlowe
{
	public class HarloweDatamap: TwineType
	{
		internal Dictionary<string,TwineVar> Map;

		public HarloweDatamap()
		{
			Map = new Dictionary<string,TwineVar>();
		}

		public HarloweDatamap(IDictionary<string,TwineVar> map)
		{
			Map = new Dictionary<string, TwineVar>(map);
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

				Map[key] = vals[i + 1];
			}
		}

		public int Length
		{
			get { return Map.Count; }
		}

		public bool ContainsKey(string key)
		{
			return Map.ContainsKey(key);
		}

		public override string ToString()
		{
			var str = new StringBuilder();
			foreach(var pair in Map.OrderBy(v => v.Key))
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
			
			if (!Map.TryGetValue(memberName, out val))
				throw new TwineTypeMemberException(string.Format("The datamap doesn't have an entry under the name '{0}'.", memberName));

			return new TwineVar(this, memberName, val);
		}

		public override void SetMember(string memberName, TwineVar value)
		{
			Map[memberName] = value;
		}

		public override void RemoveMember(string memberName)
		{
			Map.Remove(memberName);
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
						new HashSet<string>(Map.Keys).SetEquals(bMap.Map.Keys) &&
						new HashSet<TwineVar>(Map.Values).SetEquals(bMap.Map.Values);
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
					var output = new Dictionary<string, TwineVar>(this.Map);
					bMap.Map.ToList().ForEach(x => output[x.Key] = x.Value);
					result = new HarloweDatamap(output);
					break;
				}
				case TwineOperator.Subtract: { 
					// Going out on a limb here, adding subtraction support to Harlowe:
					// Treat any set as keys to remove from the dictionary

					IEnumerable<string> keys;
					if (b is HarloweDatamap)
						keys = ((HarloweDatamap)b).Map.Keys;
					else if (b is HarloweDataset)
						keys = ((HarloweDataset)b).Values.Select(v => v.ToString());
					else if (b is HarloweArray)
						keys = ((HarloweArray)b).Values.Select(v => v.ToString());
					else
						return false;
					
					var keySet = new HashSet<string>(keys);

					// A little linq-fu
					result = new HarloweDatamap(this.Map
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