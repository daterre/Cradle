using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cradle;

namespace Cradle
{
	public class DoubleService: VarTypeService<double>
	{
		public override StoryVar GetMember(double container, StoryVar member)
		{
			throw new VarTypeMemberException("Cannot directly get any members of a number.");
		}

		public override void SetMember(double container, StoryVar member, StoryVar value)
		{
			throw new VarTypeMemberException("Cannot directly set any members of a number.");
		}

		public override void RemoveMember(double container, StoryVar member)
		{
			throw new VarTypeMemberException("Cannot directly remove any members of a number.");
		}

		// ............................

		public override bool Compare(Operator op, double a, object b, out bool result)
		{
			result = false;

			double bDouble;
			if (!StoryVar.TryConvertTo<double>(b, out bDouble))
				return false;

			switch(op)
			{
				case Operator.Equals:
					result = a == bDouble; break;
				case Operator.GreaterThan:
					result = a > bDouble; break;
				case Operator.GreaterThanOrEquals:
					result = a >= bDouble; break;
				case Operator.LessThan:
					result = a < bDouble; break;
				case Operator.LessThanOrEquals:
					result = a <= bDouble; break;
				default:
					return false; // comparison not possible
			}
			return true;
		}

		public override bool Combine(Operator op, double a, object b, out StoryVar result)
		{
			result = default(StoryVar);

			// Logical operators, treat as bool
			if (op == Operator.LogicalAnd || op == Operator.LogicalOr)
			{
				bool aBool;
				return ConvertTo<bool>(a, out aBool) && StoryVar.GetTypeService<bool>(true).Combine(op, aBool, b, out result);
			}

			double bDouble;
			if (!StoryVar.TryConvertTo<double>(b, out bDouble))
				return false;

			switch (op)
			{
				case Operator.Add:
					result = a + bDouble; break;
				case Operator.Subtract:
					result = a - bDouble; break;
				case Operator.Multiply:
					result = a * bDouble; break;
				case Operator.Divide:
					result = a / bDouble; break;
				case Operator.Modulo:
					result = a % bDouble; break;
				default:
					return false; // combination not possible
			}
			return true;
		}

		public override bool Unary(Operator op, double a, out StoryVar result)
		{
			result = default(StoryVar);

			switch (op)
			{
				case Operator.Increment:
					result = ++a; break;
				case Operator.Decrement:
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
