using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityTwine;

namespace UnityTwine
{
	public class DoubleService: TwineTypeService<double>
	{
		public override TwineVar GetMember(double container, TwineVar member)
		{
			throw new TwineTypeMemberException("Cannot directly get any members of a number.");
		}

		public override void SetMember(double container, TwineVar member, TwineVar value)
		{
			throw new TwineTypeMemberException("Cannot directly set any members of a number.");
		}

		public override void RemoveMember(double container, TwineVar member)
		{
			throw new TwineTypeMemberException("Cannot directly remove any members of a number.");
		}

		// ............................

		public override bool Compare(TwineOperator op, double a, object b, out bool result)
		{
			result = false;

			double bDouble;
			if (!TwineVar.TryConvertTo<double>(b, out bDouble))
				return false;

			switch(op)
			{
				case TwineOperator.Equals:
					result = a == bDouble; break;
				case TwineOperator.GreaterThan:
					result = a > bDouble; break;
				case TwineOperator.GreaterThanOrEquals:
					result = a >= bDouble; break;
				case TwineOperator.LessThan:
					result = a < bDouble; break;
				case TwineOperator.LessThanOrEquals:
					result = a <= bDouble; break;
				default:
					return false; // comparison not possible
			}
			return true;
		}

		public override bool Combine(TwineOperator op, double a, object b, out TwineVar result)
		{
			result = default(TwineVar);

			// Logical operators, treat as bool
			if (op == TwineOperator.LogicalAnd || op == TwineOperator.LogicalOr)
			{
				bool aBool;
				return ConvertTo<bool>(a, out aBool) && TwineVar.GetTypeService<bool>(true).Combine(op, aBool, b, out result);
			}

			double bDouble;
			if (!TwineVar.TryConvertTo<double>(b, out bDouble))
				return false;

			switch (op)
			{
				case TwineOperator.Add:
					result = a + bDouble; break;
				case TwineOperator.Subtract:
					result = a - bDouble; break;
				case TwineOperator.Multiply:
					result = a * bDouble; break;
				case TwineOperator.Divide:
					result = a / bDouble; break;
				case TwineOperator.Modulo:
					result = a % bDouble; break;
				default:
					return false; // combination not possible
			}
			return true;
		}

		public override bool Unary(TwineOperator op, double a, out TwineVar result)
		{
			result = default(TwineVar);

			switch (op)
			{
				case TwineOperator.Increment:
					result = ++a; break;
				case TwineOperator.Decrement:
					result = --a; break;
				default:
					return false;
			}
			return true;
		}

		public override bool ConvertTo(double a, Type t, out object result, bool strict = false)
		{
			result = null;
			if (t == typeof(double))
			{
				result = a;
			}
			else if (t == typeof(string))
			{
				if (strict)
					return false;
				result = a.ToString();
			}
			else if (t == typeof(int))
			{
				try { result = Convert.ToInt32(a); }
				catch { return false; }
			}
			else if (t == typeof(bool))
				result = a != 0.0;
			else
				return false;

			return true;
		}

		public override bool ConvertFrom(object a, out double result, bool strict = false)
		{
			result = 0d;
			if (a == null)
				return true;
			else
				return false;

		}

		public override double Duplicate(double value)
		{
			return value;
		}
	}
}
