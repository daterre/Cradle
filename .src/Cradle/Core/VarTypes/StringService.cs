using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cradle;

namespace Cradle
{
	public class StringService: VarTypeService<string>
	{
		public override StoryVar GetMember(string container, StoryVar member)
		{
			string containerString = (string)container;

			StoryVar value;

			int pos;
			if (StoryVar.TryConvertTo<int>(member, out pos))
				value = containerString[pos];
			else if (member.ToString() == "length")
				value = containerString.Length;
			else
				value = default(StoryVar);

			return value;
		}

		public override void SetMember(string container, StoryVar member, StoryVar value)
		{
			throw new VarTypeMemberException("Cannot directly set any members of a string.");
		}

		public override void RemoveMember(string container, StoryVar member)
		{
			throw new VarTypeMemberException("Cannot directly remove any members of a string.");
		}

		// ............................

		public override bool Compare(Operator op, string a, object b, out bool result)
		{
			result = false;

			// Logical operators, treat as bool
			if (op == Operator.LogicalAnd || op == Operator.LogicalOr)
			{
				bool aBool;
				return ConvertTo<bool>(a, out aBool) && StoryVar.GetTypeService<bool>(true).Compare(op, aBool, b, out result);
			}

			// Try numeric comparison (in strict mode this won't work)
			double aDouble;
			if (ConvertTo<double>(a, out aDouble) && StoryVar.GetTypeService<double>(true).Compare(op, aDouble, b, out result))
				return true;

			string bString;
			if (!StoryVar.TryConvertTo<string>(b, out bString))
				return false;

			switch(op)
			{
				case Operator.Equals:
					result = a == bString; break;
				case Operator.GreaterThan:
					result = String.Compare(a, bString) > 0; break;
				case Operator.GreaterThanOrEquals:
					result = String.Compare(a, bString) >= 0; break;
				case Operator.LessThan:
					result = String.Compare(a, bString) < 0; break;
				case Operator.LessThanOrEquals:
					result = String.Compare(a, bString) <= 0; break;
				case Operator.Contains:
					result = a.Contains(bString); break;
				default:
					return false;
			}

			return true;
		}

		public override bool Combine(Operator op, string a, object b, out StoryVar result)
		{
			result = default(StoryVar);

			// Logical operators, treat as bool
			if (op == Operator.LogicalAnd || op == Operator.LogicalOr)
			{
				bool aBool;
				return ConvertTo<bool>(a, out aBool) && StoryVar.GetTypeService<bool>(true).Combine(op, aBool, b, out result);
			}

			// To comply with Javascript comparison, concat if b is a string
			if (op == Operator.Add && b is string)
			{
				result = a + (string)b;
				return true;
			}

			// For other operators, try numeric comabinations if possible (in strict mode this won't work)
			double aDouble;
			if (ConvertTo<double>(a, out aDouble))
				return StoryVar.GetTypeService<double>(true).Combine(op, aDouble, b, out result);
			else
				return false;
		}

		public override bool Unary(Operator op, string a, out StoryVar result)
		{
			result = default(StoryVar);

			// Try numeric unary if possible (in strict mode this won't work)
			double aDouble;
			if (ConvertTo(a, out aDouble))
				return StoryVar.GetTypeService<double>(true).Unary(op, aDouble, out result);
			else
				return false;
		}

		public override bool ConvertTo(string a, Type t, out object result, bool strict = false)
		{
			result = null;
			if (t == typeof(string))
			{
				result = a;
			}
			else if (t == typeof(double))
			{
				if (strict)
					return false;

				double d;
				if (!double.TryParse(a, out d))
					return false;
				result = d;
			}
			else if (t == typeof(int))
			{
				if (strict)
					return false;

				int i;
				if (!int.TryParse(a, out i))
					return false;
				result = i;
			}
			else if (t == typeof(bool))
				result = a != null;
			else
				return false;

			return true;
		}

		public override bool ConvertFrom(object a, out string result, bool strict = false)
		{
			result = null;

			if (a == null)
			{
				return true;
			}
			else
				return false;
		}

		public override string Duplicate(string value)
		{
			return value;
		}
	}
}
