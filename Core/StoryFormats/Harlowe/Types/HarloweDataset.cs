using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace UnityTwine.StoryFormats.Harlowe
{
	public class HarloweDataset : HarloweCollection
	{
		internal HashSet<TwineVar> Values;

		public HarloweDataset()
		{
			Values = new HashSet<TwineVar>();
		}

		public HarloweDataset(params TwineVar[] vals):this((IEnumerable<TwineVar>)vals)
		{
		}

		public HarloweDataset(IEnumerable<TwineVar> vals)
		{
			Values = new HashSet<TwineVar>(HarloweSpread.Flatten(vals));
		}

		public int Length
		{
			get { return Values.Count; }
		}

		public bool Contains(object value)
		{
			return Values.Contains(value is TwineVar ? (TwineVar)value : new TwineVar(value));
		}

        public override IEnumerable<TwineVar> GetValues()
		{
            foreach (TwineVar val in Values)
                yield return val;
		}

		public override ITwineType Clone()
		{
            return new HarloweDataset(this.GetValues().Select(v => v.Clone()));
		}

		public override string ToString()
		{
			var str = new StringBuilder();
			foreach(TwineVar value in Values.OrderBy(v => v))
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
				throw new TwineTypeMemberException("Datasets can't be accessed by position.");
		}

		public override TwineVar GetMember(string memberName)
		{
			TwineVar val;
			if (memberName.ToLower() == "length")
			{
				val = this.Length;
			}
			else
			{
				EnsureNotPosition(memberName);
				throw new TwineTypeMemberException(string.Format("The dataset doesn't have a member called {0}.", memberName));
			}

			return new TwineVar(val);
		}

		public override void SetMember(string memberName, TwineVar value)
		{
			if (memberName.ToLower() == "length")
				throw new TwineTypeMemberException("'length' cannot be modified.");

			EnsureNotPosition(memberName);
			throw new TwineTypeMemberException(string.Format("The dataset doesn't have a member called {0}.", memberName));
		}

		public override void RemoveMember(string memberName)
		{
			if (memberName.ToLower() == "length")
				throw new TwineTypeMemberException("'length' cannot be modified.");

			EnsureNotPosition(memberName);
			throw new TwineTypeMemberException(string.Format("The dataset doesn't have a member called {0}.", memberName));
		}

		public override bool Compare(TwineOperator op, object b, out bool result)
		{
			result = false;

			switch (op)
			{
				case TwineOperator.Equals: {
					if (!(b is HarloweDataset))
						return false;

					var bSet = (HarloweDataset)b;
					result = Values.SetEquals(bSet.Values);
					break;
				}
				case TwineOperator.Contains: {
					result = this.Contains(b);
					break;
				}
			}

			return true;
		}

		public override bool Combine(TwineOperator op, object b, out TwineVar result)
		{
			result = default(TwineVar);
			if (!(b is HarloweDataset))
				return false;
			var bSet = (HarloweDataset)b;

			switch (op)
			{
				case TwineOperator.Add:
					result = new TwineVar(new HarloweDataset(Values.Concat(bSet.Values)));
					break;
				case TwineOperator.Subtract:
					result = new TwineVar(new HarloweDataset(Values.Except(bSet.Values)));
					break;
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