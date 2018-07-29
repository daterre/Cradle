using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cradle;

namespace Cradle.StoryFormats.Sugar
{
	public class SugarDictionaryService : VarTypeService<Dictionary<string, StoryVar>>
	{
		public override bool Combine(Operator op, Dictionary<string, StoryVar> a, object b, out StoryVar result)
		{
			// Not supported, return default value and false
			result = default(StoryVar);
			return false;
		}

		public override bool Compare(Operator op, Dictionary<string, StoryVar> a, object b, out bool result)
		{
			switch(op)
			{
				case Operator.Equals: result = Object.Equals(a, b); return true;
				default: result = false; return false;
			}
		}

		public override bool ConvertFrom(object a, out Dictionary<string, StoryVar> result, bool strict = false)
		{
			// Not supported, return default value and false
			result = null;
			return false;
		}

		public override bool ConvertTo(Dictionary<string, StoryVar> a, Type t, out object result, bool strict = false)
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

		public override Dictionary<string, StoryVar> Duplicate(Dictionary<string, StoryVar> value)
		{
			return new Dictionary<string, StoryVar>(value);
		}

		public override StoryVar GetMember(Dictionary<string, StoryVar> container, StoryVar member)
		{
			StoryVar value;

			string key;
			if (!StoryVar.TryConvertTo<string>(member, out key) || !container.TryGetValue(key, out value))
				value = default(StoryVar);

			return value;
		}

		public override void RemoveMember(Dictionary<string, StoryVar> container, StoryVar member)
		{
			string key;
			if (StoryVar.TryConvertTo<string>(member, out key))
				container.Remove(key);
		}

		public override void SetMember(Dictionary<string, StoryVar> container, StoryVar member, StoryVar value)
		{
			string key;
			if (StoryVar.TryConvertTo<string>(member, out key))
				container[key] = value;
		}

		public override bool Unary(Operator op, Dictionary<string, StoryVar> a, out StoryVar result)
		{
			throw new NotSupportedException();
		}
	}
}
