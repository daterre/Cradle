using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnityTwine.StoryFormats.Harlowe
{
	public class Spread: TwineType
	{
		public HarloweArray Target;

		public Spread(TwineVar val)
		{
			if (!(val.Value is HarloweArray))
				throw new TwineTypeException("Only an array, datamap or dataset can be spread");

			this.Target = (HarloweArray) val.Value;
		}

		public static implicit operator Spread(TwineVar val)
		{
			return new Spread(val);
		}

		public override TwineVar GetMember(string memberName)
		{
			throw new System.NotSupportedException();
		}

		public override void SetMember(string memberName, TwineVar value)
		{
			throw new System.NotSupportedException();
		}

		public override void RemoveMember(string memberName)
		{
			throw new System.NotSupportedException();
		}

		public override bool Compare(TwineOperator op, object b, out bool result)
		{
			throw new System.NotSupportedException();
		}

		public override bool Combine(TwineOperator op, object b, out TwineVar result)
		{
			throw new System.NotSupportedException();
		}

		public override bool Unary(TwineOperator op, out TwineVar result)
		{
			throw new System.NotSupportedException();
		}

		public override bool ConvertTo(System.Type t, out object result, bool strict = false)
		{
			throw new System.NotSupportedException();
		}
	}
}