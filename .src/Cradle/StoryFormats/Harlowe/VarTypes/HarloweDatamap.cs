using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace Cradle.StoryFormats.Harlowe
{
	public class HarloweDatamap : HarloweCollection
	{
		internal Dictionary<string,StoryVar> Dictionary;

		public HarloweDatamap()
		{
			Dictionary = new Dictionary<string,StoryVar>();
		}

		public HarloweDatamap(IDictionary<string,StoryVar> map)
		{
			Dictionary = new Dictionary<string, StoryVar>(map);
		}

		public HarloweDatamap(params StoryVar[] vals):this()
		{
			if (vals.Length % 2 != 0)
				throw new VarTypeException("To create a datamap you must pass an even number of parameters.");
			for (int i = 0; i < vals.Length; i+=2)
			{
				string key;
				if (!StoryVar.TryConvertTo<string>(vals[i], out key))
					throw new VarTypeException("To create a datamap, every odd parameter (an entry name) must be string.");

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

        public override IEnumerable<StoryVar> GetValues()
		{
            foreach (StoryVar val in Dictionary.Values)
                yield return val;
		}

		public override IVarType Duplicate()
		{
			var clone = new HarloweDatamap();
			foreach (var pair in this.Dictionary)
				clone.Dictionary[pair.Key] = pair.Value.Duplicate();

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

		public override StoryVar GetMember(StoryVar member)
		{
			StoryVar val;
			if (TryGetMemberArray(member, out val))
				return val;

			var memberName = member.ToString();
			
			if (!Dictionary.TryGetValue(memberName, out val))
				throw new VarTypeMemberException(string.Format("The datamap doesn't have an entry under the name '{0}'.", memberName));

			return new StoryVar(val);
		}

		public override void SetMember(StoryVar member, StoryVar value)
		{
			var memberName = member.ToString();

			Dictionary[memberName] = value.Duplicate();
		}

		public override void RemoveMember(StoryVar member)
		{
			var memberName = member.ToString();
			Dictionary.Remove(memberName);
		}

		public override bool Compare(Operator op, object b, out bool result)
		{
			result = false;

			switch (op)
			{
				case Operator.Equals: {
					if (!(b is HarloweDatamap))
						return false;

					var bMap = (HarloweDatamap)b;
					result =
						new HashSet<string>(Dictionary.Keys).SetEquals(bMap.Dictionary.Keys) &&
						new HashSet<StoryVar>(Dictionary.Values).SetEquals(bMap.Dictionary.Values);
					break;
				}
				case Operator.Contains: {
					result = this.ContainsKey(StoryVar.ConvertTo<string>(b));
					break;
				}
			}

			return true;
		}

		public override bool Combine(Operator op, object b, out StoryVar result)
		{
			result = default(StoryVar);

			switch (op)
			{
				case Operator.Add: {
					if (!(b is HarloweDatamap))
						return false;
					var bMap = (HarloweDatamap)b;
					var output = new Dictionary<string, StoryVar>(this.Dictionary);
					bMap.Dictionary.ToList().ForEach(x => output[x.Key] = x.Value);
					result = new HarloweDatamap(output);
					break;
				}
				case Operator.Subtract: { 
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

		public override bool Unary(Operator op, out StoryVar result)
		{
			result = default(StoryVar);
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