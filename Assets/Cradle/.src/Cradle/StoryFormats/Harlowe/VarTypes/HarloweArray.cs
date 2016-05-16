using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace Cradle.StoryFormats.Harlowe
{
	public class HarloweArray: HarloweCollection
	{
		internal List<StoryVar> Values;

		public HarloweArray()
		{
			Values = new List<StoryVar>();
		}

		public HarloweArray(params StoryVar[] vals):this((IEnumerable<StoryVar>)vals)
		{
		}

		public HarloweArray(IEnumerable<StoryVar> vals)
		{
			Values = new List<StoryVar>(HarloweSpread.Flatten(vals));
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
			foreach (StoryVar val in this.Values)
				yield return val.Duplicate();
		}

		public override IVarType Duplicate()
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

		public override StoryVar GetMember(StoryVar member)
		{
			StoryVar val;
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
						throw new VarTypeMemberException(string.Format("The array doesn't have a {0} position.", memberName));
					}
				}
				else
					throw new VarTypeMemberException(string.Format("The array doesn't have a member called {0}.", memberName));
			}

			return new StoryVar(val);
		}

		public override void SetMember(StoryVar member, StoryVar value)
		{
			var memberName = member.ToString().ToLower();
			if (memberName == "length")
				throw new VarTypeMemberException("'length' cannot be modified.");

			int index;
			if (HarloweUtils.TryPositionToIndex(memberName, Values.Count, out index))
			{
				try { Values[index] = value.Duplicate(); }
				catch (System.IndexOutOfRangeException)
				{
					throw new VarTypeMemberException(string.Format("The array doesn't have a {0} position.", memberName));
				}
			}
			else
				throw new VarTypeMemberException(string.Format("The array doesn't have a member called {0}.", memberName));
		}

		public override void RemoveMember(StoryVar member)
		{
			var memberName = member.ToString().ToLower();
			if (memberName == "length")
				throw new VarTypeMemberException("'length' cannot be modified.");

			int index;
			if (HarloweUtils.TryPositionToIndex(memberName, Values.Count, out index))
			{
				try { Values.RemoveAt(index); }
				catch (System.IndexOutOfRangeException)
				{
					throw new VarTypeMemberException(string.Format("The array doesn't have a {0} position.", memberName));
				}
			}
			else
				throw new VarTypeMemberException(string.Format("The array doesn't have a member called {0}.", memberName));
		}

		public override bool Compare(Operator op, object b, out bool result)
		{
			result = false;

			switch (op)
			{
				case Operator.Equals: {
					if (!(b is HarloweArray))
						return false;
					var bArray = (HarloweArray)b;

					result = Values.SequenceEqual(bArray.Values);
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
			if (!(b is HarloweArray))
				return false;
			var bArray = (HarloweArray)b;

			switch (op)
			{
				case Operator.Add:
					result = new StoryVar(new HarloweArray(Values.Concat(bArray.Values)));
					break;
				case Operator.Subtract:
					result = new StoryVar(new HarloweArray(Values.Except(bArray.Values)));
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