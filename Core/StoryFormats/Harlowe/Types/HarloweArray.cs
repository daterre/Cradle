using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnityTwine.StoryFormats.Harlowe
{
	public class HarloweArray: TwineType
	{
		List<TwineVar> values;

		public HarloweArray()
		{
			values = new List<TwineVar>();
		}

		public HarloweArray(params TwineVar[] vals)
		{
			values = new List<TwineVar>(vals);
		}

		public int Length
		{
			get { return values.Count; }
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
				int index = HarloweUtils.PositionToIndex(memberName, values.Count);
				try { val = values[index]; }
				catch (System.IndexOutOfRangeException)
				{
					throw new System.IndexOutOfRangeException(string.Format("The array doesn't have a {0} position."));
				}
			}

			return new TwineVar(this, memberName, val);
		}

		public override void SetMember(string memberName, TwineVar value)
		{
			if (memberName.ToLower() == "length")
				throw new TwineTypeMemberException("Cannot directly set the length of an array.");

			int index = HarloweUtils.PositionToIndex(memberName, values.Count);
			try { values[index] = value; }
			catch (System.IndexOutOfRangeException)
			{
				throw new System.IndexOutOfRangeException(string.Format("The array doesn't have a {0} position."));
			}
		}

		public override void RemoveMember(string memberName)
		{
			throw new System.NotImplementedException();
		}

		public override bool Compare(TwineOperator op, object b, out bool result)
		{
			throw new System.NotImplementedException();
		}

		public override bool Combine(TwineOperator op, object b, out TwineVar result)
		{
			throw new System.NotImplementedException();
		}

		public override bool Unary(TwineOperator op, out TwineVar result)
		{
			throw new System.NotImplementedException();
		}

		public override bool ConvertTo(System.Type t, out object result, bool strict = false)
		{
			throw new System.NotImplementedException();
		}
	}
}