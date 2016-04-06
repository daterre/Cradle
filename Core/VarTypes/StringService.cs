using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityTwine;

namespace UnityTwine
{
	public class StringService: TwineTypeService<string>
	{
		public override TwineVar GetMember(string container, string memberName)
		{
			string containerString = (string)container;

			TwineVar value;

			switch (memberName.ToLower())
			{
				case "length":
					value = containerString.Length; break;
				default:
					value = containerString[Int32.Parse(memberName)]; break;
			}

			return new TwineVar(container, memberName, value);
		}

		public override void SetMember(string container, string memberName, TwineVar value)
		{
			throw new TwineTypeMemberException("Cannot directly set any members of a string.");
		}

		public override void RemoveMember(string container, string memberName)
		{
			throw new TwineTypeMemberException("Cannot directly remove any members of a string.");
		}

		// ............................

		public override bool Compare(TwineOperator op, string a, object b, out bool result)
		{
			result = false;

			// Logical operators, treat as bool
			if (op == TwineOperator.LogicalAnd || op == TwineOperator.LogicalOr)
			{
				bool aBool;
				return ConvertTo<bool>(a, out aBool) && TwineVar.GetTypeService<bool>(true).Compare(op, aBool, b, out result);
			}

			// Try numeric comparison (in strict mode this won't work)
			double aDouble;
			if (ConvertTo<double>(a, out aDouble) && TwineVar.GetTypeService<double>(true).Compare(op, aDouble, b, out result))
				return true;

			string bString;
			if (!TwineVar.TryConvertTo<string>(b, out bString))
				return false;

			switch(op)
			{
				case TwineOperator.Equals:
					result = a == bString; break;
				case TwineOperator.GreaterThan:
					result = String.Compare(a, bString) > 0; break;
				case TwineOperator.GreaterThanOrEquals:
					result = String.Compare(a, bString) >= 0; break;
				case TwineOperator.LessThan:
					result = String.Compare(a, bString) < 0; break;
				case TwineOperator.LessThanOrEquals:
					result = String.Compare(a, bString) <= 0; break;
				case TwineOperator.Contains:
					result = a.Contains(bString); break;
				default:
					return false;
			}

			return true;
		}

		public override bool Combine(TwineOperator op, string a, object b, out TwineVar result)
		{
			result = default(TwineVar);

			// Logical operators, treat as bool
			if (op == TwineOperator.LogicalAnd || op == TwineOperator.LogicalOr)
			{
				bool aBool;
				return ConvertTo<bool>(a, out aBool) && TwineVar.GetTypeService<bool>(true).Combine(op, aBool, b, out result);
			}

			// To comply with Javascript comparison, concat if b is a string
			if (op == TwineOperator.Add && b is string)
			{
				result = a + (string)b;
				return true;
			}

			// For other operators, try numeric comabinations if possible (in strict mode this won't work)
			double aDouble;
			if (ConvertTo<double>(a, out aDouble))
				return TwineVar.GetTypeService<double>(true).Combine(op, aDouble, b, out result);
			else
				return false;
		}

		public override bool Unary(TwineOperator op, string a, out TwineVar result)
		{
			result = default(TwineVar);

			// Try numeric unary if possible (in strict mode this won't work)
			double aDouble;
			if (ConvertTo(a, out aDouble))
				return TwineVar.GetTypeService<double>(true).Unary(op, aDouble, out result);
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
			result = null;
			if (t == typeof(int))
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
	}
}
