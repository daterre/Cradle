using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityTwine;

namespace UnityTwine
{
	public class IntService: TwineTypeService<int>
	{
		/// <summary>
		/// All numeric operations are done as floating-point numbers, but are converted back to integers if the result is a whole number within this precision.
		/// </summary>
		public const int DecimalPrecision = 5;

		public override TwineVar GetMember(int container, TwineVar member)
		{
			throw new TwineTypeMemberException("Cannot directly get any members of a number.");
		}

		public override void SetMember(int container, TwineVar member, TwineVar value)
		{
			throw new TwineTypeMemberException("Cannot directly set any members of a number.");
		}

		public override void RemoveMember(int container, TwineVar member)
		{
			throw new TwineTypeMemberException("Cannot directly remove any members of a number.");
		}

		// ............................

		public override bool Compare(TwineOperator op, int a, object b, out bool result)
		{
			// Always use double comparison
			result = false;
			double aDouble;
			return ConvertTo(a, out aDouble) && TwineVar.GetTypeService<double>(true).Compare(op, aDouble, b, out result);
		}

		public override bool Combine(TwineOperator op, int a, object b, out TwineVar result)
		{
			result = default(TwineVar);

			// Logical operators, treat as bool
			if (op == TwineOperator.LogicalAnd || op == TwineOperator.LogicalOr)
			{
				bool aBool;
				return ConvertTo<bool>(a, out aBool) && TwineVar.GetTypeService<bool>(true).Combine(op, aBool, b, out result);
			}

			// Always use double combinations
			double aDouble;
			if (!ConvertTo<double>(a, out aDouble) || !TwineVar.GetTypeService<double>(true).Combine(op, aDouble, b, out result))
				return false;

			// Convert result back to int if it is whole number (or close to it in precision)
			double resultDouble = (double)result.Value;
			if (Math.Round(resultDouble) == Math.Round(resultDouble, DecimalPrecision))
				result.Value = Convert.ToInt32(resultDouble);

			return true;
		}

		public override bool Unary(TwineOperator op, int a, out TwineVar result)
		{
			result = default(TwineVar);

			// Always use double operations
			double aDouble;
			if (!ConvertTo(a, out aDouble) || !TwineVar.GetTypeService<double>(true).Unary(op, aDouble, out result))
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
