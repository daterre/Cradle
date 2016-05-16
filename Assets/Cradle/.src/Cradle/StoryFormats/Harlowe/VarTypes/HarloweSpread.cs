using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Cradle.StoryFormats.Harlowe
{
	public class HarloweSpread: VarType
	{
		public HarloweCollection Target;

		public HarloweSpread(object val)
		{
			object v = val is StoryVar ? ((StoryVar)val).Value : val;

			if (!(v is HarloweCollection))
				throw new VarTypeException("Only an array, datamap or dataset can be spread");

			this.Target = (HarloweCollection)v;
		}

		public static IEnumerable<StoryVar> Flatten(IEnumerable<StoryVar> vals)
		{
			foreach(StoryVar val in vals)
			{
				if (val.Value is HarloweSpread)
				{
					var spread = (HarloweSpread)val.Value;
                    foreach (StoryVar innerVal in spread.Target.GetValues())
                        yield return innerVal.Duplicate();

				}
				else
                    yield return val.Duplicate();
			}
		}

		public static explicit operator HarloweSpread(StoryVar val)
		{
			return new HarloweSpread(val);
		}

		public override StoryVar GetMember(StoryVar member)
		{
			throw new System.NotSupportedException();
		}

		public override void SetMember(StoryVar member, StoryVar value)
		{
			throw new System.NotSupportedException();
		}

		public override void RemoveMember(StoryVar member)
		{
			throw new System.NotSupportedException();
		}

		public override bool Compare(Operator op, object b, out bool result)
		{
			throw new System.NotSupportedException();
		}

		public override bool Combine(Operator op, object b, out StoryVar result)
		{
			throw new System.NotSupportedException();
		}

		public override bool Unary(Operator op, out StoryVar result)
		{
			throw new System.NotSupportedException();
		}

		public override bool ConvertTo(System.Type t, out object result, bool strict = false)
		{
			throw new System.NotSupportedException();
		}

		public override IVarType Duplicate()
		{
			return new HarloweSpread(this.Target);
		}
	}
}