using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cradle;

namespace Cradle.StoryFormats.Sugar
{
	public class SugarListService : VarTypeService<List<StoryVar>>
	{
		public override bool Combine(Operator op, List<StoryVar> a, object b, out StoryVar result)
		{
			// Not supported, return default value and false
			result = default(StoryVar);
			return false;
		}

		public override bool Compare(Operator op, List<StoryVar> a, object b, out bool result)
		{
			switch(op)
			{
				case Operator.Equals: result = Object.Equals(a, b); return true;
				default: result = false; return false;
			}
		}

		public override bool ConvertFrom(object a, out List<StoryVar> result, bool strict = false)
		{
			// Not supported, return default value and false
			result = null;
			return false;
		}

		public override bool ConvertTo(List<StoryVar> a, Type t, out object result, bool strict = false)
		{
			if (t == typeof(string))
			{
				result = string.Join(",", a.Select(v => v.ToString()).ToArray());
				return true;
			}

			// Other conversions not supported, return default value and false
			result = null;
			return false;
		}

		public override List<StoryVar> Duplicate(List<StoryVar> value)
		{
			return new List<StoryVar>(value);
		}

		public override StoryVar GetMember(List<StoryVar> container, StoryVar member)
		{
			StoryVar value;

			int pos;
			if (StoryVar.TryConvertTo<int>(member, out pos))
				value = container[pos];
			else
				value = default(StoryVar);

			return value;
		}

		public override void RemoveMember(List<StoryVar> container, StoryVar member)
		{
			int pos;
			if (StoryVar.TryConvertTo<int>(member, out pos))
				container.RemoveAt(pos);
		}

		public override void SetMember(List<StoryVar> container, StoryVar member, StoryVar value)
		{
			int pos;
			if (StoryVar.TryConvertTo<int>(member, out pos))
				container[pos] = value;
		}

		public override bool Unary(Operator op, List<StoryVar> a, out StoryVar result)
		{
			throw new NotSupportedException();
		}
	}
}
