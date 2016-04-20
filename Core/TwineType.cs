using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UnityTwine
{
	public interface ITwineType
	{
		TwineVar GetMember(string memberName);
		void SetMember(string memberName, TwineVar value);
		void RemoveMember(string memberName);

		bool Compare(TwineOperator op, object b, out bool result);
		bool Combine(TwineOperator op, object b, out TwineVar result);
		bool Unary(TwineOperator op, out TwineVar result);
		bool ConvertTo(Type t, out object result, bool strict = false);
		ITwineType Clone();
	}

	public abstract class TwineType: ITwineType
	{
		public abstract TwineVar GetMember(string memberName);
		public abstract void SetMember(string memberName, TwineVar value);
		public abstract void RemoveMember(string memberName);

		public abstract bool Compare(TwineOperator op, object b, out bool result);
		public abstract bool Combine(TwineOperator op, object b, out TwineVar result);
		public abstract bool Unary(TwineOperator op, out TwineVar result);
		public abstract bool ConvertTo(Type t, out object result, bool strict = false);
		public abstract ITwineType Clone();
	}

	public interface ITwineTypeService
	{
		TwineVar GetMember(object container, string memberName);
		void SetMember(object container, string memberName, TwineVar value);
		void RemoveMember(object container, string memberName);

		bool Compare(TwineOperator op, object a, object b, out bool result);
		bool Combine(TwineOperator op, object a, object b, out TwineVar result);
		bool Unary(TwineOperator op, object a, out TwineVar result);
		bool ConvertTo(object a, Type t, out object result, bool strict = false);
		bool ConvertFrom(object a, out object result, bool strict = false);
		object Clone(object value);
	}

	public abstract class TwineTypeService<T>: ITwineTypeService
	{
		public abstract TwineVar GetMember(T container, string memberName);
		public abstract void SetMember(T container, string memberName, TwineVar value);
		public abstract void RemoveMember(T container, string memberName);

		public abstract bool Compare(TwineOperator op, T a, object b, out bool result);
		public abstract bool Combine(TwineOperator op, T a, object b, out TwineVar result);
		public abstract bool Unary(TwineOperator op, T a, out TwineVar result);
		public abstract bool ConvertTo(T a, Type t, out object result, bool strict = false);
		public abstract bool ConvertFrom(object a, out T result, bool strict = false);
		public abstract T Clone(T value);

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

		bool ITwineTypeService.Compare(TwineOperator op, object a, object b, out bool result)
		{
			return Compare(op, (T)a, b, out result);
		}

		bool ITwineTypeService.Combine(TwineOperator op, object a, object b, out TwineVar result)
		{
			return Combine(op, (T)a, b, out result);
		}

		bool ITwineTypeService.Unary(TwineOperator op, object a, out TwineVar result)
		{			
			return Unary(op, (T)a, out result);
		}

		bool ITwineTypeService.ConvertTo(object a, Type t, out object result, bool strict)
		{
			return ConvertTo((T)a, t, out result, strict);
		}

		bool ITwineTypeService.ConvertFrom(object a, out object result, bool strict)
		{
			T resultT;
			bool success = ConvertFrom(a, out resultT, strict);
			result = resultT;
			return success;
		}

		TwineVar ITwineTypeService.GetMember(object container, string memberName)
		{
			return GetMember((T)container, memberName);
		}

		void ITwineTypeService.SetMember(object container, string memberName, TwineVar value)
		{
			SetMember((T)container, memberName, value);
		}

		void ITwineTypeService.RemoveMember(object container, string memberName)
		{
			RemoveMember((T)container, memberName);
		}

		object ITwineTypeService.Clone(object value)
		{
			return Clone((T)value);
		}
	}

	public enum TwineOperator
	{
		Equals,
		GreaterThan,
		GreaterThanOrEquals,
		LessThan,
		LessThanOrEquals,
		Contains,
		//ContainedBy,
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
