using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace UnityTwine.StoryFormats.Harlowe
{
	public class HarloweArray: TwineType
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

		public override TwineVar GetMember(string memberName)
		{
			TwineVar val;
			if (memberName.ToLower() == "length")
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

			return new TwineVar(this, memberName, val);
		}

		public override void SetMember(string memberName, TwineVar value)
		{
			if (memberName.ToLower() == "length")
				throw new TwineTypeMemberException("'length' cannot be modified.");

			int index;
			if (HarloweUtils.TryPositionToIndex(memberName, Values.Count, out index))
			{
				try { Values[index] = value; }
				catch (System.IndexOutOfRangeException)
				{
					throw new TwineTypeMemberException(string.Format("The array doesn't have a {0} position.", memberName));
				}
			}
			else
				throw new TwineTypeMemberException(string.Format("The array doesn't have a member called {0}.", memberName));
		}

		public override void RemoveMember(string memberName)
		{
			if (memberName.ToLower() == "length")
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