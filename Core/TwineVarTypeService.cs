using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UnityTwine
{
	public interface ITwineVarType
	{
		//bool Contains(TwineVar val);

		TwineVar this[string propertyName]
		{
			get;
			set;
		}

		bool CompareLeft(TwineOperator op, object b);
		bool CompareRight(TwineOperator op, object a);
		bool CombineLeft(TwineOperator op, object b, out TwineVar result);
		bool CombineRight(TwineOperator op, object a, out TwineVar result);

	}

	public abstract class TwineVarTypeService
	{
		public abstract TwineVar GetProperty(object container, string propertyName);
		public abstract void SetProperty(object container, string propertyName, TwineVar value);
		//public abstract bool Contains(object container, object containee);

		public abstract bool Compare(TwineOperator op, object a, object b);
		public abstract bool Combine(TwineOperator op, object a, object b, out TwineVar result);
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
		Plus,
		Minus,
		Multiply,
		Divide,
		Modulo,
	}
}
