using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnityTwine.StoryFormats.Harlowe
{
	public class HarloweSpread: TwineType
	{
		public HarloweCollection Target;

		public HarloweSpread(object val)
		{
			object v = val is TwineVar ? ((TwineVar)val).Value : val;

			if (!(v is HarloweCollection))
				throw new TwineTypeException("Only an array, datamap or dataset can be spread");

			this.Target = (HarloweCollection)v;
		}

		public static IEnumerable<TwineVar> Flatten(IEnumerable<TwineVar> vals)
		{
			foreach(TwineVar val in vals)
			{
				if (val.Value is HarloweSpread)
				{
					var spread = (HarloweSpread)val.Value;
                    foreach (TwineVar innerVal in spread.Target.GetValues())
                        yield return innerVal.Clone();

				}
				else
                    yield return val.Clone();
			}
		}

		public static explicit operator HarloweSpread(TwineVar val)
		{
			return new HarloweSpread(val);
		}

		public override TwineVar GetMember(TwineVar member)
		{
			throw new System.NotSupportedException();
		}

		public override void SetMember(TwineVar member, TwineVar value)
		{
			throw new System.NotSupportedException();
		}

		public override void RemoveMember(TwineVar member)
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

		public override ITwineType Clone()
		{
			return new HarloweSpread(this.Target);
		}
	}
}