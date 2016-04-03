using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UnityTwine
{
	public interface ITwineType
	{
		TwineVar this[string propertyName]
		{
			get;
			set;
		}

		bool Compare(TwineOperator op, object b, out bool result);
		bool Combine(TwineOperator op, object b, out TwineVar result);
		bool Unary(TwineOperator op, out TwineVar result);
		bool ConvertTo(Type t, out object result, bool strict = false);
	}

	public interface ITwineTypeService
	{
		TwineVar GetProperty(object container, string propertyName);
		void SetProperty(object container, string propertyName, TwineVar value);

		bool Compare(TwineOperator op, object a, object b, out bool result);
		bool Combine(TwineOperator op, object a, object b, out TwineVar result);
		bool Unary(TwineOperator op, object a, out TwineVar result);
		bool ConvertTo(object a, Type t, out object result, bool strict = false);
	}

	public abstract class TwineTypeService<T>: ITwineTypeService
	{
		public abstract TwineVar GetProperty(T container, string propertyName);
		public abstract void SetProperty(T container, string propertyName, TwineVar value);

		public abstract bool Compare(TwineOperator op, T a, object b, out bool result);
		public abstract bool Combine(TwineOperator op, T a, object b, out TwineVar result);
		public abstract bool Unary(TwineOperator op, T a, out TwineVar result);
		public abstract bool ConvertTo(T a, Type t, out object result, bool strict = false);

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

		TwineVar ITwineTypeService.GetProperty(object container, string propertyName)
		{
			return GetProperty((T)container, propertyName);
		}

		void ITwineTypeService.SetProperty(object container, string propertyName, TwineVar value)
		{
			SetProperty((T)container, propertyName, value);
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
		ContainedBy,
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
