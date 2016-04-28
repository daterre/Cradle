using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace UnityTwine.StoryFormats.Harlowe
{
	public class HarloweArray: HarloweCollection
	{
		internal List<TwineVar> Values;

		public HarloweArray()
		{
			Values = new List<TwineVar>();
		}

		public HarloweArray(params TwineVar[] vals):this((IEnumerable<TwineVar>)vals)
		{
		}

		public HarloweArray(IEnumerable<TwineVar> vals)
		{
			Values = new List<TwineVar>(HarloweSpread.Flatten(vals));
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
			foreach (TwineVar val in this.Values)
				yield return val.Duplicate();
		}

		public override ITwineType Duplicate()
		{
            return new HarloweArray(this.GetValues().Select(v => v.Duplicate()));
		}

		public override string ToString()
		{
			var str = new StringBuilder();
			for (int i = 0; i < Values.Count; i++)
			{
				str.Append(Values[i].ToString());
				if (i < Values.Count - 1)
					str.Append(',');
			}

			return str.ToString();
		}

		public override TwineVar GetMember(TwineVar member)
		{
			TwineVar val;
			if (TryGetMemberArray(member, out val))
				return val;

			var memberName = member.ToString().ToLower();
			if (memberName == "length")
			{
				val = this.Length;
			}
			else
			{
				int index;
				if (HarloweUtils.TryPositionToIndex(memberName, Values.Count, out index))
				{
					try { val = Values[index]; }
					catch (System.IndexOutOfRangeException)
					{
						throw new TwineTypeMemberException(string.Format("The array doesn't have a {0} position.", memberName));
					}
				}
				else
					throw new TwineTypeMemberException(string.Format("The array doesn't have a member called {0}.", memberName));
			}

			return new TwineVar(val);
		}

		public override void SetMember(TwineVar member, TwineVar value)
		{
			var memberName = member.ToString().ToLower();
			if (memberName == "length")
				throw new TwineTypeMemberException("'length' cannot be modified.");

			int index;
			if (HarloweUtils.TryPositionToIndex(memberName, Values.Count, out index))
			{
				try { Values[index] = value.Duplicate(); }
				catch (System.IndexOutOfRangeException)
				{
					throw new TwineTypeMemberException(string.Format("The array doesn't have a {0} position.", memberName));
				}
			}
			else
				throw new TwineTypeMemberException(string.Format("The array doesn't have a member called {0}.", memberName));
		}

		public override void RemoveMember(TwineVar member)
		{
			var memberName = member.ToString().ToLower();
			if (memberName == "length")
				throw new TwineTypeMemberException("'length' cannot be modified.");

			int index;
			if (HarloweUtils.TryPositionToIndex(memberName, Values.Count, out index))
			{
				try { Values.RemoveAt(index); }
				catch (System.IndexOutOfRangeException)
				{
					throw new TwineTypeMemberException(string.Format("The array doesn't have a {0} position.", memberName));
				}
			}
			else
				throw new TwineTypeMemberException(string.Format("The array doesn't have a member called {0}.", memberName));
		}

		public override bool Compare(TwineOperator op, object b, out bool result)
		{
			result = false;

			switch (op)
			{
				case TwineOperator.Equals: {
					if (!(b is HarloweArray))
						return false;
					var bArray = (HarloweArray)b;

					result = Values.SequenceEqual(bArray.Values);
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
			if (!(b is HarloweArray))
				return false;
			var bArray = (HarloweArray)b;

			switch (op)
			{
				case TwineOperator.Add:
					result = new TwineVar(new HarloweArray(Values.Concat(bArray.Values)));
					break;
				case TwineOperator.Subtract:
					result = new TwineVar(new HarloweArray(Values.Except(bArray.Values)));
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