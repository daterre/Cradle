using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cradle;

namespace Cradle
{
	public class BoolService: VarTypeService<bool>
	{
		public override StoryVar GetMember(bool container, StoryVar member)
		{
			throw new VarTypeMemberException("Cannot directly get any members of a boolean.");
		}

		public override void SetMember(bool container, StoryVar member, StoryVar value)
		{
			throw new VarTypeMemberException("Cannot directly set any members of a boolean.");
		}

		public override void RemoveMember(bool container, StoryVar member)
		{
			throw new VarTypeMemberException("Cannot directly remove any members of a boolean.");
		}

		// ............................

		public override bool Compare(Operator op, bool a, object b, out bool result)
		{
			result = false;

			if (op == Operator.Equals)
			{
				// Equaliy, evaluate as bools
				bool bBool;
				if (!StoryVar.TryConvertTo<bool>(b, out bBool))
					return false;

				result = a == bBool;
				return true;
			}
			else
			{
				// Evaluate as numbers for other operators
				double aDouble;
				return ConvertTo<double>(a, out aDouble) && StoryVar.GetTypeService<double>(true).Compare(op, aDouble, b, out result);
			}
		}

		public override bool Combine(Operator op, bool a, object b, out StoryVar result)
		{
			result = default(StoryVar);

			if (op == Operator.LogicalAnd || op == Operator.LogicalOr)
			{
				bool bBool;
				if (!StoryVar.TryConvertTo<bool>(b, out bBool))
					return false;
				
				switch(op)
				{
					case Operator.LogicalAnd:
						result = a && bBool; break;
					case Operator.LogicalOr:
						result = a || bBool; break;
					default:
						break;
				}
				return true;
			}

			double aDouble;
			return ConvertTo<double>(a, out aDouble) && StoryVar.GetTypeService<double>(true).Combine(op, aDouble, b, out result);
		}

		public override bool Unary(Operator op, bool a, out StoryVar result)
		{
			result = default(StoryVar);

			double aDouble;
			return ConvertTo<double>(a, out aDouble) && StoryVar.GetTypeService<double>(true).Unary(op, aDouble, out result);
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

		public override bool Duplicate(bool value)
		{
			return value;
		}
	}
}
