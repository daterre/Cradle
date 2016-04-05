using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnityTwine
{
	/// <summary>
	/// Comaptiable with TwineVar, this type is used to remember the container from which the var was taken in case it needs to be moved out.
	/// </summary>
	public struct TwineVarRef : ITwineVar
	{
		public TwineVar Container;
		public string Member;
		public TwineVar Value;

		public TwineVarRef(object container, string member)
		{
			Container = new TwineVar(container);
			Member = member;
			Value = default(TwineVar);
		}

		public TwineVarRef(object container, string member, TwineVar value)
		{
			Container = new TwineVar(container);
			Member = member;
			Value = value;
		}

		public TwineVarRef(TwineVar value)
		{
			Container = default(TwineVar);
			Member = null;
			Value = value;
		}

		public static implicit operator TwineVar(TwineVarRef varRef)
		{
			return varRef.Value;
		}

		public static implicit operator TwineVarRef(TwineVar val)
		{
			return new TwineVarRef(null, null, val);
		}

		public TwineVarRef GetMember(string memberName)
		{
			return Value.GetMember(memberName);
		}

		public TwineVarRef GetMember(TwineVar memberName)
		{
			return Value.GetMember(memberName);
		}

		public void SetMember(string memberName, TwineVar val)
		{
			Value.SetMember(memberName, val);
		}

		public void SetMember(TwineVar memberName, TwineVar val)
		{
			Value.SetMember(memberName, val);
		}

		public TwineVarRef this[string memberName]
		{
			get
			{
				return Value[memberName];
			}
			set
			{
				Value[memberName] = value;
			}
		}

		public TwineVarRef AsMemberOf(TwineVar val)
		{
			return Value.AsMemberOf(val);
		}

		public bool Contains(object obj)
		{
			return Value.Contains(obj);
		}

		public bool ContainedBy(object obj)
		{
			return Value.ContainedBy(obj);
		}

		public void ApplyValue(TwineVar val)
		{
			if (this.Container.Value != null)
				this.Container.SetMember(this.Member, val);
			this.Value = val;
		}

		public void PutInto(TwineVarRef varRef)
		{
			Value.PutInto(varRef);
		}

		public void MoveInto(TwineVarRef varRef)
		{
			varRef.ApplyValue(this.Value);

			if (this.Container.Value != null)
				this.Container.RemoveMember(this.Member);

			this.Container = default(TwineVar);
			this.Member = null;
			this.Value = default(TwineVar);
		}


		#region Operators
		// ------------------------

		public static TwineVarRef operator ++(TwineVarRef vRef)
		{
			// Unary operator returns an empty ref, but the the target index setter will actually reassign a proper ref to itself
			return new TwineVarRef(null, null, TwineVar.Unary(TwineOperator.Increment, vRef.Value));
		}

		public static TwineVarRef operator --(TwineVarRef val)
		{
			// Unary operator returns an empty ref, but the the target index setter will actually reassign a proper ref to itself
			return new TwineVarRef(null, null, TwineVar.Unary(TwineOperator.Decrement, val.Value));
		}

		public static bool operator ==(TwineVarRef a, object b)
		{
			return TwineVar.Compare(TwineOperator.Equals, a.Value, b);
		}

		public static bool operator !=(TwineVarRef a, object b)
		{
			return !(a.Value == b);
		}

		public static bool operator >(TwineVarRef a, object b)
		{
			return TwineVar.Compare(TwineOperator.GreaterThan, a.Value, b);
		}

		public static bool operator >=(TwineVarRef a, object b)
		{
			return TwineVar.Compare(TwineOperator.GreaterThanOrEquals, a.Value, b);
		}

		public static bool operator <(TwineVarRef a, object b)
		{
			return TwineVar.Compare(TwineOperator.LessThan, a.Value, b);
		}

		public static bool operator <=(TwineVarRef a, object b)
		{
			return TwineVar.Compare(TwineOperator.LessThanOrEquals, a.Value, b);
		}

		public static TwineVar operator +(TwineVarRef a, object b)
		{
			return TwineVar.Combine(TwineOperator.Add, a.Value, b);
		}

		public static TwineVar operator -(TwineVarRef a, object b)
		{
			return TwineVar.Combine(TwineOperator.Subtract, a.Value, b);
		}

		public static TwineVar operator *(TwineVarRef a, object b)
		{
			return TwineVar.Combine(TwineOperator.Multiply, a.Value, b);
		}

		public static TwineVar operator /(TwineVarRef a, object b)
		{
			return TwineVar.Combine(TwineOperator.Divide, a.Value, b);
		}

		public static TwineVar operator %(TwineVarRef a, object b)
		{
			return TwineVar.Combine(TwineOperator.Modulo, a.Value, b);
		}

		public static TwineVar operator &(TwineVarRef a, TwineVarRef b)
		{
			return TwineVar.Combine(TwineOperator.LogicalAnd, a.Value, b.Value);
		}

		public static TwineVar operator |(TwineVarRef a, TwineVarRef b)
		{
			return TwineVar.Combine(TwineOperator.LogicalOr, a.Value, b.Value);
		}

		public static implicit operator string(TwineVarRef val)
		{
			return val.Value.ConvertTo<string>();
		}

		public static implicit operator TwineVarRef(string val)
		{
			return new TwineVarRef(val);
		}

		public static implicit operator double(TwineVarRef val)
		{
			return val.Value.ConvertTo<double>();
		}

		public static implicit operator TwineVarRef(double val)
		{
			return new TwineVarRef(val);
		}

		public static implicit operator int(TwineVarRef val)
		{
			return val.Value.ConvertTo<int>();
		}

		public static implicit operator TwineVarRef(int val)
		{
			return new TwineVarRef(val);
		}

		public static implicit operator bool(TwineVarRef val)
		{
			return val.Value.ConvertTo<bool>();
		}

		public static implicit operator TwineVarRef(bool val)
		{
			return new TwineVarRef(val);
		}

		public static bool operator true(TwineVarRef val)
		{
			return val.Value.ConvertTo<bool>();
		}

		public static bool operator false(TwineVarRef val)
		{
			return !val.Value.ConvertTo<bool>();
		}
		// ------------------------
		#endregion
	}
}

