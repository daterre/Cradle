using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cradle
{
	public interface IVarType
	{
		StoryVar GetMember(StoryVar member);
		void SetMember(StoryVar member, StoryVar value);
		void RemoveMember(StoryVar member);

		bool Compare(Operator op, object b, out bool result);
		bool Combine(Operator op, object b, out StoryVar result);
		bool Unary(Operator op, out StoryVar result);
		bool ConvertTo(Type t, out object result, bool strict = false);
		IVarType Duplicate();
	}

	public abstract class VarType: IVarType
	{
		public abstract StoryVar GetMember(StoryVar member);
		public abstract void SetMember(StoryVar member, StoryVar value);
		public abstract void RemoveMember(StoryVar member);

		public abstract bool Compare(Operator op, object b, out bool result);
		public abstract bool Combine(Operator op, object b, out StoryVar result);
		public abstract bool Unary(Operator op, out StoryVar result);
		public abstract bool ConvertTo(Type t, out object result, bool strict = false);
		public abstract IVarType Duplicate();
	}

	public interface IVarTypeService
	{
		StoryVar GetMember(object container, StoryVar member);
		void SetMember(object container, StoryVar member, StoryVar value);
		void RemoveMember(object container, StoryVar member);

		bool Compare(Operator op, object a, object b, out bool result);
		bool Combine(Operator op, object a, object b, out StoryVar result);
		bool Unary(Operator op, object a, out StoryVar result);
		bool ConvertTo(object a, Type t, out object result, bool strict = false);
		bool ConvertFrom(object a, out object result, bool strict = false);
		object Duplicate(object value);
	}

	public abstract class VarTypeService<T>: IVarTypeService
	{
		public abstract StoryVar GetMember(T container, StoryVar member);
		public abstract void SetMember(T container, StoryVar member, StoryVar value);
		public abstract void RemoveMember(T container, StoryVar member);

		public abstract bool Compare(Operator op, T a, object b, out bool result);
		public abstract bool Combine(Operator op, T a, object b, out StoryVar result);
		public abstract bool Unary(Operator op, T a, out StoryVar result);
		public abstract bool ConvertTo(T a, Type t, out object result, bool strict = false);
		public abstract bool ConvertFrom(object a, out T result, bool strict = false);
		public abstract T Duplicate(T value);

		public bool ConvertTo<ResultT>(T a, out ResultT resultT)
		{
			resultT = default(ResultT);
			object result;
			if (ConvertTo(a, typeof(ResultT), out result))
			{
				resultT = (ResultT)result;
				return true;
			}
			else
				return false;
		}

		bool IVarTypeService.Compare(Operator op, object a, object b, out bool result)
		{
			return Compare(op, (T)a, b, out result);
		}

		bool IVarTypeService.Combine(Operator op, object a, object b, out StoryVar result)
		{
			return Combine(op, (T)a, b, out result);
		}

		bool IVarTypeService.Unary(Operator op, object a, out StoryVar result)
		{			
			return Unary(op, (T)a, out result);
		}

		bool IVarTypeService.ConvertTo(object a, Type t, out object result, bool strict)
		{
			return ConvertTo((T)a, t, out result, strict);
		}

		bool IVarTypeService.ConvertFrom(object a, out object result, bool strict)
		{
			T resultT;
			bool success = ConvertFrom(a, out resultT, strict);
			result = resultT;
			return success;
		}

		StoryVar IVarTypeService.GetMember(object container, StoryVar member)
		{
			return GetMember((T)container, member);
		}

		void IVarTypeService.SetMember(object container, StoryVar member, StoryVar value)
		{
			SetMember((T)container, member, value);
		}

		void IVarTypeService.RemoveMember(object container, StoryVar member)
		{
			RemoveMember((T)container, member);
		}

		object IVarTypeService.Duplicate(object value)
		{
			return Duplicate((T)value);
		}
	}

	public enum Operator
	{
		Equals,
		GreaterThan,
		GreaterThanOrEquals,
		LessThan,
		LessThanOrEquals,
		Contains,
		Add,
		Subtract,
		Multiply,
		Divide,
		Modulo,
		Increment,
		Decrement,
		LogicalAnd,
		LogicalOr
	}
}
