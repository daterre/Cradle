using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace Cradle.StoryFormats.Harlowe
{
	public class HarloweDataset : HarloweCollection
	{
		internal HashSet<StoryVar> Values;

		public HarloweDataset()
		{
			Values = new HashSet<StoryVar>();
		}

		public HarloweDataset(params StoryVar[] vals):this((IEnumerable<StoryVar>)vals)
		{
		}

		public HarloweDataset(IEnumerable<StoryVar> vals)
		{
			Values = new HashSet<StoryVar>(HarloweSpread.Flatten(vals));
		}

		public int Length
		{
			get { return Values.Count; }
		}

		public bool Contains(object value)
		{
			return Values.Contains(value is StoryVar ? (StoryVar)value : new StoryVar(value));
		}

        public override IEnumerable<StoryVar> GetValues()
		{
            foreach (StoryVar val in Values)
                yield return val;
		}

		public override IVarType Duplicate()
		{
            return new HarloweDataset(this.GetValues().Select(v => v.Duplicate()));
		}

		public override string ToString()
		{
			var str = new StringBuilder();
			foreach(StoryVar value in Values.OrderBy(v => v))
			{
				if (str.Length > 0)
					str.Append(',');
				str.Append(value.ToString());
			}

			return str.ToString();
		}

		void EnsureNotPosition(string memberName)
		{
			int index;
			if (HarloweUtils.TryPositionToIndex(memberName, Values.Count, out index))
				throw new VarTypeMemberException("Datasets can't be accessed by position.");
		}

		public override StoryVar GetMember(StoryVar member)
		{
			var memberName = member.ToString().ToLower();
			if (memberName == "length")
				return this.Length;

			EnsureNotPosition(memberName);
			throw new VarTypeMemberException(string.Format("The dataset doesn't have a member called {0}.", memberName));
		}

		public override void SetMember(StoryVar member, StoryVar value)
		{
			var memberName = member.ToString().ToLower();
			if (memberName == "length")
				throw new VarTypeMemberException("'length' cannot be modified.");

			EnsureNotPosition(memberName);
			throw new VarTypeMemberException(string.Format("The dataset doesn't have a member called {0}.", memberName));
		}

		public override void RemoveMember(StoryVar member)
		{
			var memberName = member.ToString().ToLower();
			if (memberName == "length")
				throw new VarTypeMemberException("'length' cannot be modified.");

			EnsureNotPosition(memberName);
			throw new VarTypeMemberException(string.Format("The dataset doesn't have a member called {0}.", memberName));
		}

		public override bool Compare(Operator op, object b, out bool result)
		{
			result = false;

			switch (op)
			{
				case Operator.Equals: {
					if (!(b is HarloweDataset))
						return false;

					var bSet = (HarloweDataset)b;
					result = Values.SetEquals(bSet.Values);
					break;
				}
				case Operator.Contains: {
					result = this.Contains(b);
					break;
				}
			}

			return true;
		}

		public override bool Combine(Operator op, object b, out StoryVar result)
		{
			result = default(StoryVar);
			if (!(b is HarloweDataset))
				return false;
			var bSet = (HarloweDataset)b;

			switch (op)
			{
				case Operator.Add:
					result = new StoryVar(new HarloweDataset(Values.Concat(bSet.Values)));
					break;
				case Operator.Subtract:
					result = new StoryVar(new HarloweDataset(Values.Except(bSet.Values)));
					break;
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