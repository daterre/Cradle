using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cradle;

namespace Cradle
{
	public class IntService: VarTypeService<int>
	{
		/// <summary>
		/// All numeric operations are done as floating-point numbers, but are converted back to integers if the result is a whole number within this precision.
		/// </summary>
		public const int DecimalPrecision = 5;

		public override StoryVar GetMember(int container, StoryVar member)
		{
			throw new VarTypeMemberException("Cannot directly get any members of a number.");
		}

		public override void SetMember(int container, StoryVar member, StoryVar value)
		{
			throw new VarTypeMemberException("Cannot directly set any members of a number.");
		}

		public override void RemoveMember(int container, StoryVar member)
		{
			throw new VarTypeMemberException("Cannot directly remove any members of a number.");
		}

		// ............................

		public override bool Compare(Operator op, int a, object b, out bool result)
		{
			// Always use double comparison
			result = false;
			double aDouble;
			return ConvertTo(a, out aDouble) && StoryVar.GetTypeService<double>(true).Compare(op, aDouble, b, out result);
		}

		public override bool Combine(Operator op, int a, object b, out StoryVar result)
		{
			result = default(StoryVar);

			// Logical operators, treat as bool
			if (op == Operator.LogicalAnd || op == Operator.LogicalOr)
			{
				bool aBool;
				return ConvertTo<bool>(a, out aBool) && StoryVar.GetTypeService<bool>(true).Combine(op, aBool, b, out result);
			}

			// Always use double combinations
			double aDouble;
			if (!ConvertTo<double>(a, out aDouble) || !StoryVar.GetTypeService<double>(true).Combine(op, aDouble, b, out result))
				return false;

			// Convert result back to int if it is whole number (or close to it in precision)
			double resultDouble = (double)result.Value;
			if (Math.Round(resultDouble) == Math.Round(resultDouble, DecimalPrecision))
				result.Value = Convert.ToInt32(resultDouble);

			return true;
		}

		public override bool Unary(Operator op, int a, out StoryVar result)
		{
			result = default(StoryVar);

			// Always use double operations
			double aDouble;
			if (!ConvertTo(a, out aDouble) || !StoryVar.GetTypeService<double>(true).Unary(op, aDouble, out result))
				return false;

			// Convert result back to int if it is whole number (or close to it in precision)
			double resultDouble = (double)result.Value;
			if (Math.Round(resultDouble) == Math.Round(resultDouble, DecimalPrecision))
				result.Value = Convert.ToInt32(resultDouble);

			return true;
		}

		public override bool ConvertTo(int a, Type t, out object result, bool strict = false)
		{
			result = null;
			if (t == typeof(int))
			{
				result = a;
			}
			else if (t == typeof(string))
			{
				if (strict)
					return false;
				result = a.ToString();
			}
			else if (t == typeof(double))
			{
				try { result = Convert.ToDouble(a); }
				catch (Exception ex) { UnityEngine.Debug.LogException(ex); return false; }
			}
			else if (t == typeof(bool))
				result = a != 0;
			else
				return false;

			return true;
		}


		public override bool ConvertFrom(object a, out int result, bool strict = false)
		{
			result = 0;
			if (a == null)
				return true;
			else
				return false;

		}

		public override int Duplicate(int value)
		{
			return value;
		}
	}
}
