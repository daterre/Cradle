using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace UnityTwine
{
	[Serializable]
	public struct TwineVar
	{
		internal object Value;

		public Type GetInnerType()
		{
			return Value == null ? null : Value.GetType();
		}

		public override int GetHashCode()
		{
			if (Value == null)
				return 0;

			int hash = 17;
			hash = hash * 31 + Value.GetType().GetHashCode();
			hash = hash * 31 + Value.GetHashCode();
			return hash;
		}

		// ..............
		// PROPERTIES

		static Dictionary<Type, TwineVarTypeService> _typeServices = new Dictionary<Type, TwineVarTypeService>();

		public static void RegisterTypeService<T>(TwineVarTypeService service)
		{
			_typeServices[typeof(T)] = service;
		}

		public TwineVar GetProperty(string propertyName)
		{
			if (Value == null)
				throw new TwineVarPropertyException("Cannot get property of empty Twine var.");

			TwineVarTypeService service;
			if (_typeServices.TryGetValue(this.Value.GetType(), out service))
				return service.GetProperty(this.Value, propertyName);

			if (this.Value is ITwineVarType)
				return ((ITwineVarType)this.Value)[propertyName];

			throw new TwineVarPropertyException(string.Format("Cannot get property of a Twine var of type {0}.", this.Value.GetType().Name));
		}

		public TwineVar GetProperty(TwineVar propertyName)
		{
			return GetProperty(propertyName.ToString());
		}

		public void SetProperty(string propertyName, TwineVar val)
		{
			if (Value == null)
				throw new TwineVarPropertyException("Cannot set property of empty Twine var.");

			TwineVarTypeService service;
			if (_typeServices.TryGetValue(this.Value.GetType(), out service))
				service.SetProperty(this.Value, propertyName, val);

			if (this.Value is ITwineVarType)
				((ITwineVarType)this.Value)[propertyName] = val;

			throw new TwineVarPropertyException(string.Format("Cannot set property of a Twine var of type {0}.", this.Value.GetType().Name));
		}

		public void SetProperty(TwineVar propertyName, TwineVar val)
		{
			SetProperty(propertyName.ToString(), val);
		}

		public TwineVar this[string propertyName]
		{
			get
			{
				return GetProperty(propertyName);
			}
			set
			{
				SetProperty(propertyName, value);
			}
		}

		public TwineVar AsPropertyOf(TwineVar val)
		{
			return val.GetProperty(this);
		}

		// ..............
		// OBJECT

		public TwineVar(object val)
		{
			this.Value = val;
		}

		internal ITwineVarType VarType
		{
			get { return this.Value as ITwineVarType; }
		}

		public static TwineVar Combine(TwineOperator op, TwineVar a, TwineVar b)
		{
			TwineVar result;

			TwineVarTypeService service;
			if ((a.Value != null && _typeServices.TryGetValue(a.Value.GetType(), out service)) ||
				(b.Value != null && _typeServices.TryGetValue(b.Value.GetType(), out service)))
			{
				if (service.Combine(op, a.Value, b.Value, out result))
					return result;
			}

			if (a.Value is ITwineVarType)
			{
				if (a.VarType.CombineLeft(op, b.Value, out result))
					return result;
			}
			if (b.Value is ITwineVarType)
			{
				if (b.VarType.CombineRight(op, a.Value, out result))
					return result;
			}

			throw new TwineVarTypeException(string.Format("Cannot use {0} with {1} and {2}", op, a.Value ?? "null", b.Value ?? "null"));
		}

		public static TwineVar Combine(TwineOperator op, TwineVar a, TwineVar b)
		{
			TwineVar result;

			TwineVarTypeService service;
			if ((a.Value != null && _typeServices.TryGetValue(a.Value.GetType(), out service)) ||
				(b.Value != null && _typeServices.TryGetValue(b.Value.GetType(), out service)))
			{
				return service.Compare(op, a.Value, b.Value);
			}

			if (a.Value is ITwineVarType)
			{
				if (a.VarType.CombineLeft(op, b.Value, out result))
					return result;
			}
			if (b.Value is ITwineVarType)
			{
				if (b.VarType.CombineRight(op, a.Value, out result))
					return result;
			}

			throw new TwineVarTypeException(string.Format("Cannot compine {0} with {1} and {2}", op, a.Value ?? "null", b.Value ?? "null"));
		}

		public override bool Equals(object obj)
		{
			object val = obj is TwineVar ? ((TwineVar)obj).Value : obj;

			if (val == null)
				return this.Value == null;
			else if (val is bool)
				return this == (bool)val;
			else if (val is int)
				return this == (int)val;
			else if (val is double)
				return this == (double)val;
			else if (val is string)
				return this == (string)val;
			else
				return val.Equals(this.Value);
		}

		public bool Contains(TwineVar val)
		{
			if (this.Value is string)
				return ((string)this.Value).Contains(val);
			else if (this.Value is ITwineVarType)
				return ((ITwineVarType)this.Value).Contains(val);
			else
				return false;
		}

		public bool ContainedBy(TwineVar val)
		{
			if (val.Value is string)
				return ((string)val.Value).Contains(this);
			else if (val.Value is ITwineVarType)
				return ((ITwineVarType)val.Value).Contains(this);
			else
				return false;
		}

		public static TwineVar operator++(TwineVar twVar)
		{
			if (twVar.Value is int)
				twVar.Value = ((int)twVar.Value)+1;
			else if (twVar.Value is double)
				twVar.Value = ((double)twVar.Value)+1;

			return twVar;
		}

		public static TwineVar operator--(TwineVar twVar)
		{
			if (twVar.Value is int)
				twVar.Value = ((int)twVar.Value)-1;
			else if (twVar.Value is double)
				twVar.Value = ((double)twVar.Value)-1;

			return twVar;
		}

		public static bool operator==(TwineVar a, TwineVar b)
		{
			return a.Equals(b);
		}

		public static bool operator!=(TwineVar a, TwineVar b)
		{
			return !a.Equals(b);
		}

		public static TwineVar operator +(TwineVar a, TwineVar b)
		{
			TwineVar result;

			if (a.Value == null && b.Value == null)
				return null;

			if (a.Value is string || b.Value is string)
				return a.ToString() + b.ToString();

			if (a.Value is double || b.Value is double)
			{
				double aVal, bVal;
				if (a.TryToDouble(out aVal) && b.TryToDouble(out bVal))
					return aVal + bVal;
			}

			if (a.Value is int || b.Value is int || a.Value is bool || b.Value is bool)
			{
				int aVal, bVal;
				if (a.TryToInt(out aVal) && b.TryToInt(out bVal))
					return aVal + bVal;
			}

			TwineVarTypeService service;
			if ((a.Value != null && _typeServices.TryGetValue(a.Value.GetType(), out service)) ||
				(b.Value != null && _typeServices.TryGetValue(b.Value.GetType(), out service)))
			{
				if (service.Combine(TwineOperator.Plus, a.Value, b.Value, out result))
					return result;
			}

			if (a.Value is ITwineVarType)
			{
				if (a.VarType.OperatorLeft(TwineVarOperator.Plus, b.Value, out result))
					return result;
			}
			if (b.Value is ITwineVarType)
			{
				if (b.VarType.OperatorRight(TwineVarOperator.Plus, a.Value, out result))
					return result;
			}

			throw new TwineVarTypeException(string.Format("Cannot add {0} to {1}", a.Value ?? "null", b.Value ?? "null"));
		}

		public static TwineVar operator -(TwineVar a, TwineVar b)
		{
			if (a.Value is double || b.Value is double)
			{
				double aVal, bVal;
				if (a.TryToDouble(out aVal) && b.TryToDouble(out bVal))
					return aVal - bVal;
			}

			if (a.Value is int || b.Value is int)
			{
				int aVal, bVal;
				if (a.TryToInt(out aVal) && b.TryToInt(out bVal))
					return aVal - bVal;
			}

			// Undefined so just return a
			throw new TwineVarTypeException("Cannot substract values of this type.");
		}

		// ..............
		// STRING

		public TwineVar(string val)
		{
			this.Value = val;
		}

		public override string ToString()
		{
			return ConvertToString(Value);
		}

		public static string ConvertToString(object value)
		{
			return value == null ? null : value.ToString();
		}

		public static implicit operator string(TwineVar twVar) {
			return twVar.ToString();
		}

		public static implicit operator TwineVar (string val) {
			return new TwineVar(val);
		}

		public static TwineVar operator+(TwineVar twVar, string val) {
			return twVar.ToString() + val;
		}

		public static bool operator==(TwineVar twVar, string val) {
			return twVar.ToString() == val;
		}

		public static bool operator!=(TwineVar twVar, string val) {
			return twVar.ToString() != val;
		}

		// ..............
		// INT

		public TwineVar(int val)
		{
			this.Value = val;
		}

		public int ToInt()
		{
			if (this.Value is int) {
				return (int) this.Value;
			}
			else if (this.Value is IConvertible)
			{
				return Convert.ToInt32(this.Value);
			}
			else
				return 0;
		}

		public static implicit operator int(TwineVar twVar) {
			return twVar.ToInt();
		}

		public static implicit operator TwineVar (int val) {
			return new TwineVar(val);
		}

		public static TwineVar operator+(TwineVar twVar, int val) {
			if (twVar.Value is string) {
				return (string) twVar.Value + val.ToString();
			}
			else if (twVar.Value is int || twVar.Value is bool) {
				return (int) twVar.Value + val;
			}
			else if (twVar.Value is double) {
				return (double) twVar.Value + (double) val;
			}
			else
				return twVar;
		}

		public static TwineVar operator-(TwineVar twVar, int val) {

			if (twVar.Value is double) {
				return (double) twVar.Value - (double) val;
			}
			else
				return twVar.ToInt() - val;
		}

		//public static TwineVar operator-(int val, TwineVar twVar)
		//{
		//	if (twVar.Value is double)
		//	{
		//		return (double)val - twVar.ToDouble();
		//	}
		//	else
		//		return val - twVar.ToInt();
		//}

		public static TwineVar operator*(TwineVar twVar, int val) {
			if (twVar.Value is double) {
				return (double) twVar.Value * (double) val;
			}
			else
				return twVar.ToInt() * val;
		}

		public static TwineVar operator *(int val, TwineVar twVar)
		{
			return twVar * val;
		}

		public static TwineVar operator/(TwineVar twVar, int val) {
			if (twVar.Value is double) {
				return (double) twVar.Value / (double) val;
			}
			else
				return twVar.ToInt() / val;
		}

		public static TwineVar operator /(int val, TwineVar twVar)
		{
			if (twVar.Value is double)
			{
				return Convert.ToDouble(val) / (double) twVar.Value;
			}
			else
				return val / twVar.ToInt();
		}

		public static TwineVar operator%(TwineVar twVar, int val) {
			if (twVar.Value is double) {
				return (double) twVar.Value % (double) val;
			}
			else
				return twVar.ToInt() % val;
		}

		//public static TwineVar operator %(int val, TwineVar twVar)
		//{
		//	if (twVar.Value is double)
		//	{
		//		return (double)val % twVar.ToDouble();
		//	}
		//	else
		//		return val % twVar.ToInt();
		//}

		public static bool operator==(TwineVar twVar, int val) {
			return twVar.ToInt() == val;
		}

		//public static bool operator ==(int val, TwineVar twVar)
		//{
		//	return twVar.ToInt() == val;
		//}

		public static bool operator!=(TwineVar twVar, int val) {
			return twVar.ToInt() != val;
		}

		//public static bool operator !=(int val, TwineVar twVar)
		//{
		//	return twVar.ToInt() != val;
		//}

		public static bool operator>(TwineVar twVar, int val) {
			if (twVar.Value is double)
				return (double)twVar.Value > Convert.ToDouble(val);
			else
				return twVar.ToInt() > val;
		}

		//public static bool operator >(int val, TwineVar twVar)
		//{
		//	return val > twVar.ToInt();
		//}

		public static bool operator<(TwineVar twVar, int val) {
			if (twVar.Value is double)
				return (double) twVar.Value < Convert.ToDouble(val);
			else
				return twVar.ToInt() < val;
		}

		//public static bool operator <(int val, TwineVar twVar)
		//{
		//	return val < twVar.ToInt();
		//}

		public static bool operator>=(TwineVar twVar, int val) {
			if (twVar.Value is double)
				return (double)twVar.Value >= Convert.ToDouble(val);
			else
				return twVar.ToInt() >= val;
		}

		//public static bool operator >=(int val, TwineVar twVar)
		//{
		//	return val >= twVar.ToInt();
		//}

		public static bool operator<=(TwineVar twVar, int val) {
			if (twVar.Value is double)
				return (double)twVar.Value <= Convert.ToDouble(val);
			else
				return twVar.ToInt() <= val;
		}

		//public static bool operator <=(int val, TwineVar twVar)
		//{
		//	return val <= twVar.ToInt();
		//}

		// ..............
		// DOUBLE

		public TwineVar(double val)
		{
			this.Value = val;
		}

		public double ToDouble()
		{
			if (this.Value is double) {
				return (double) this.Value;
			}
			else if (this.Value is IConvertible) {
				return Convert.ToDouble(this.Value);
			}
			else
				return 0.0;
		}

		public static implicit operator double(TwineVar twVar) {
			return twVar.ToDouble();
		}

		public static implicit operator TwineVar (double val) {
			return new TwineVar(val);
		}

		public static TwineVar operator+(TwineVar twVar, double val) {
			if (twVar.Value is string) {
				return (string) twVar.Value  + val.ToString();
			}
			else {
				return twVar.ToDouble() + (double) val;
			}
		}

		//public static TwineVar operator+(double val, TwineVar twVar) {
		//	return twVar + val;
		//}

		public static TwineVar operator-(TwineVar twVar, double val) {
			return twVar.ToDouble() - val;
		}

		//public static TwineVar operator -(double val, TwineVar twVar)
		//{
		//	return val - twVar.ToDouble();
		//}

		public static TwineVar operator*(TwineVar twVar, double val) {
			return twVar.ToDouble() * val;
		}

		//public static TwineVar operator *(double val, TwineVar twVar)
		//{
		//	return twVar * val;
		//}

		public static TwineVar operator/(TwineVar twVar, double val) {
			return twVar.ToDouble() / val;
		}

		//public static TwineVar operator /(double val, TwineVar twVar)
		//{
		//	return val / twVar.ToDouble();
		//}

		public static TwineVar operator%(TwineVar twVar, double val) {
			return twVar.ToDouble() % val;
		}

		//public static TwineVar operator %(double val, TwineVar twVar)
		//{
		//	return val % twVar.ToDouble();
		//}

		public static bool operator==(TwineVar twVar, double val) {
			return twVar.ToDouble() == val;
		}

		//public static bool operator ==(double val, TwineVar twVar)
		//{
		//	return twVar.ToDouble() == val;
		//}

		public static bool operator!=(TwineVar twVar, double val) {
			return twVar.ToDouble() != val;
		}

		//public static bool operator !=(double val, TwineVar twVar)
		//{
		//	return twVar.ToDouble() != val;
		//}

		public static bool operator>(TwineVar twVar, double val) {
			return twVar.ToDouble() > val;
		}

		//public static bool operator >(double val, TwineVar twVar)
		//{
		//	return val > twVar.ToDouble();
		//}

		public static bool operator<(TwineVar twVar, double val) {
			return twVar.ToDouble() < val;
		}

		//public static bool operator <(double val, TwineVar twVar)
		//{
		//	return val < twVar.ToDouble();
		//}

		public static bool operator>=(TwineVar twVar, double val) {
			return twVar.ToDouble() >= val;
		}

		//public static bool operator >=(double val, TwineVar twVar)
		//{
		//	return val >= twVar.ToDouble();
		//}

		public static bool operator<=(TwineVar twVar, double val) {
			return twVar.ToDouble() <= val;
		}

		//public static bool operator <=(double val, TwineVar twVar)
		//{
		//	return val <= twVar.ToDouble();
		//}

		// ..............
		// BOOL

		public TwineVar(bool val)
		{
			this.Value = val;
		}

		public bool ToBool()
		{
			if (this.Value is bool) {
				return (bool) this.Value;
			}
			else if (this.Value is string) {
				return ((string)this.Value).Length > 0;
			}
			else if (this.Value is IConvertible) {
				return Convert.ToBoolean(this.Value);
			}
			else
				return false;
		}

		public static implicit operator bool(TwineVar twVar) {
			return twVar.ToBool();
		}

		public static implicit operator TwineVar (bool val) {
			return new TwineVar(val);
		}

		public static bool operator true(TwineVar twVar)
		{
			return twVar.ToBool();
		}

		public static bool operator false(TwineVar twVar)
		{
			return !twVar.ToBool();
		}

		public static bool operator==(TwineVar twVar, bool val) {
			return twVar.ToBool() == val;
		}

		public static bool operator!=(TwineVar twVar, bool val) {
			return twVar.ToBool() != val;
		}

		public static TwineVar operator&(TwineVar a, TwineVar b)
		{
			return a.ToBool() && b.ToBool();
		}

		public static TwineVar operator|(TwineVar a, TwineVar b)
		{
			return a.ToBool() || b.ToBool();
		}
	}
}

