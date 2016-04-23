using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityTwine;

namespace UnityTwine
{
	public class BoolService: TwineTypeService<bool>
	{
		public override TwineVar GetMember(bool container, TwineVar member)
		{
			throw new TwineTypeMemberException("Cannot directly get any members of a boolean.");
		}

		public override void SetMember(bool container, TwineVar member, TwineVar value)
		{
			throw new TwineTypeMemberException("Cannot directly set any members of a boolean.");
		}

		public override void RemoveMember(bool container, TwineVar member)
		{
			throw new TwineTypeMemberException("Cannot directly remove any members of a boolean.");
		}

		// ............................

		public override bool Compare(TwineOperator op, bool a, object b, out bool result)
		{
			if (op == TwineOperator.Equals && b is bool)
			{
				result = a == (bool)b;
				return true;
			}

			// Use double comparison for other operators
			result = false;
			double aDouble;
			return ConvertTo<double>(a, out aDouble) && TwineVar.GetTypeService<double>(true).Compare(op, aDouble, b, out result);
		}

		public override bool Combine(TwineOperator op, bool a, object b, out TwineVar result)
		{
			result = default(TwineVar);

			if (op == TwineOperator.LogicalAnd || op == TwineOperator.LogicalOr)
			{
				bool bBool;
				if (!TwineVar.TryConvertTo<bool>(b, out bBool))
					return false;
				
				switch(op)
				{
					case TwineOperator.LogicalAnd:
						result = a && bBool; break;
					case TwineOperator.LogicalOr:
						result = a || bBool; break;
					default:
						break;
				}
				return true;
			}

			double aDouble;
			return ConvertTo<double>(a, out aDouble) && TwineVar.GetTypeService<double>(true).Combine(op, aDouble, b, out result);
		}

		public override bool Unary(TwineOperator op, bool a, out TwineVar result)
		{
			result = default(TwineVar);

			double aDouble;
			return ConvertTo<double>(a, out aDouble) && TwineVar.GetTypeService<double>(true).Unary(op, aDouble, out result);
		}

		public override bool ConvertTo(bool a, Type t, out object result, bool strict = false)
		{
			result = null;
			if (t == typeof(bool))
			{
				result = a;
			}
			else if (t == typeof(string))
			{
				result = a ? "true" : "false";
			}
			else if (t == typeof(double) || t == typeof(int))
			{
				result = a ? 1 : 0;
			}
			else
				return false;

			return true;
		}

		public override bool ConvertFrom(object a, out bool result, bool strict = false)
		{
			result = false;
			if (a == null)
				return true;
			else
				return false;
				
		}

		public override bool Clone(bool value)
		{
			return value;
		}
	}
}
