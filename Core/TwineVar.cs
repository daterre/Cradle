using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace UnityTwine
{
	[Serializable]
	public struct TwineVar
	{
		public static bool StrictMode = false;

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

		public override string ToString()
		{
			string result;
			if (!TryConvertTo<string>(this.Value, out result))
				result = string.Empty;

			return result;
		}

		// ..............
		// TYPE SERVICES

		static Dictionary<Type, ITwineTypeService> _typeServices = new Dictionary<Type, ITwineTypeService>();

		public static void RegisterTypeService<T>(ITwineTypeService service)
		{
			_typeServices[typeof(T)] = service;
		}

		public static TwineTypeService<T> GetTypeService<T>(bool throwException = false)
		{
			var service = (TwineTypeService<T>)GetTypeService(typeof(T));
			if (service == null)
				throw new TwineException(string.Format("UnityTwine is missing a TwineTypeService for {0}. Did you mess with something you shouldn't have?", typeof(T).FullName));

			return service;
		}

		public static ITwineTypeService GetTypeService(Type t)
		{
			ITwineTypeService service = null;
			_typeServices.TryGetValue(t, out service);
			return service;
		}

		// ..............
		// PROPERTIES

		public TwineVar GetProperty(string propertyName)
		{
			if (Value == null)
				throw new TwineTypePropertyException("Cannot get property of empty Twine var.");

			ITwineTypeService service = GetTypeService(this.Value.GetType());
			if (service != null)
				return service.GetProperty(this.Value, propertyName);

			if (this.Value is ITwineType)
				return ((ITwineType)this.Value)[propertyName];

			throw new TwineTypePropertyException(string.Format("Cannot get property of a Twine var of type {0}.", this.Value.GetType().Name));
		}

		public TwineVar GetProperty(TwineVar propertyName)
		{
			return GetProperty(propertyName.ToString());
		}

		public void SetProperty(string propertyName, TwineVar val)
		{
			if (Value == null)
				throw new TwineTypePropertyException("Cannot set property of empty Twine var.");

			ITwineTypeService service = GetTypeService(this.Value.GetType());
			if (service != null)
				service.SetProperty(this.Value, propertyName, val);

			if (this.Value is ITwineType)
				((ITwineType)this.Value)[propertyName] = val;

			throw new TwineTypePropertyException(string.Format("Cannot set property of a Twine var of type {0}.", this.Value.GetType().Name));
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

		public static bool Compare(TwineOperator op, object left, object right)
		{
			object a = left is TwineVar ? ((TwineVar)left).Value : left;
			object b = right is TwineVar ? ((TwineVar)right).Value : right;

			bool result;
			ITwineTypeService service;

			if (a != null && _typeServices.TryGetValue(a.GetType(), out service) && service.Compare(op, a, b, out result))
				return result;

			if (b != null && _typeServices.TryGetValue(b.GetType(), out service) && service.Compare(op, a, b, out result))
				return result;

			if (a is ITwineType)
			{
				if ((a as ITwineType).Compare(op, b, out result))
					return result;
			}
			if (b is ITwineType)
			{
				if ((b as ITwineType).Compare(op, a, out result))
					return result;
			}

			return false;
		}

		public static bool TryCombine(TwineOperator op, object left, object right, out TwineVar result)
		{
			object a = left is TwineVar ? ((TwineVar)left).Value : left;
			object b = right is TwineVar ? ((TwineVar)right).Value : right;

			ITwineTypeService service;

			if (a != null && _typeServices.TryGetValue(a.GetType(), out service) && service.Combine(op, a, b, out result))
				return true;

			if (b != null && _typeServices.TryGetValue(b.GetType(), out service) && service.Combine(op, a, b, out result))
				return true;

			if (a is ITwineType)
			{
				if ((a as ITwineType).Combine(op, b, out result))
					return true;
			}
			if (b is ITwineType)
			{
				if ((b as ITwineType).Combine(op, a, out result))
					return true;
			}

			result = default(TwineVar);
			return false;
		}

		public static TwineVar Combine(TwineOperator op, object left, object right)
		{
			object a = left is TwineVar ? ((TwineVar)left).Value : left;
			object b = right is TwineVar ? ((TwineVar)right).Value : right;

			TwineVar result;
			if (TryCombine(op, a, b, out result))
				return result;
			else
				throw new TwineTypeException(string.Format("Cannot combine {0} with {1} using {2}",
					a == null ? "null" : a.GetType().Name,
					b == null ? "null" : b.GetType().Name,
					op
				));
		}

		public static TwineVar Unary(TwineOperator op, object obj)
		{
			object a = obj is TwineVar ? ((TwineVar)obj).Value : obj;

			ITwineTypeService service;

			TwineVar result;
			if (a != null && _typeServices.TryGetValue(a.GetType(), out service) && service.Unary(op, a, out result))
				return result;

			if (a is ITwineType)
			{
				if ((a as ITwineType).Unary(op, out result))
					return result;
			}

			throw new TwineTypeException(string.Format("Cannot use {0} with {1}", op, a.GetType().Name ?? "null"));
		}

		public static bool TryConvertTo<T>(object obj, out T result)
		{
			// Same type
			if (obj is T)
			{
				result = (T) obj;
				return true;
			}

			ITwineTypeService service = obj == null ? null : GetTypeService(obj.GetType());
			object r;
			if (service != null && service.ConvertTo(obj, typeof(T), out r, TwineVar.StrictMode))
			{
				result = (T)r;
				return true;
			}

			if (obj is ITwineType)
			{
				if ((obj as ITwineType).ConvertTo(typeof(T), out r, TwineVar.StrictMode))
				{
					result = (T)r;
					return true;
				}
			}

			result = default(T);
			return false;
		}

		public T ConvertTo<T>()
		{
			T result;
			if (TryConvertTo<T>(this.Value, out result))
				return result;
			else
				throw new TwineTypeException(string.Format("Cannot convert {0} to {1}", this.Value.GetType().Name ?? "null", typeof(T).Name));
		}

		public override bool Equals(object obj)
		{
			return Compare(TwineOperator.Equals, this, obj);
		}

		public bool Contains(object obj)
		{
			return Compare(TwineOperator.Contains, this, obj);
		}

		public bool ContainedBy(object obj)
		{
			return Compare(TwineOperator.ContainedBy, this, obj);
		}

		public static TwineVar operator++(TwineVar val)
		{
			return Unary(TwineOperator.Increment, val.Value);
		}

		public static TwineVar operator--(TwineVar val)
		{
			return Unary(TwineOperator.Decrement, val.Value);
		}

		public static bool operator==(TwineVar a, object b)
		{
			return Compare(TwineOperator.Equals, a, b);
		}

		public static bool operator!=(TwineVar a, object b)
		{
			return !(a == b);
		}

		public static bool operator >(TwineVar a, object b)
		{
			return Compare(TwineOperator.GreaterThan, a, b);
		}

		public static bool operator >=(TwineVar a, object b)
		{
			return Compare(TwineOperator.GreaterThanOrEquals, a, b);
		}

		public static bool operator <(TwineVar a, object b)
		{
			return Compare(TwineOperator.LessThan, a, b);
		}

		public static bool operator <=(TwineVar a, object b)
		{
			return Compare(TwineOperator.LessThanOrEquals, a, b);
		}

		public static TwineVar operator +(TwineVar a, object b)
		{
			return Combine(TwineOperator.Add, a, b);
		}

		public static TwineVar operator -(TwineVar a, object b)
		{
			return Combine(TwineOperator.Subtract, a, b);
		}

		public static TwineVar operator *(TwineVar a, object b)
		{
			return Combine(TwineOperator.Multiply, a, b);
		}

		public static TwineVar operator /(TwineVar a, object b)
		{
			return Combine(TwineOperator.Divide, a, b);
		}

		public static TwineVar operator %(TwineVar a, object b)
		{
			return Combine(TwineOperator.Modulo, a, b);
		}

		public static TwineVar operator &(TwineVar a, TwineVar b)
		{
			return Combine(TwineOperator.LogicalAnd, a, b);
		}

		public static TwineVar operator |(TwineVar a, TwineVar b)
		{
			return Combine(TwineOperator.LogicalOr, a, b);
		}

		public static implicit operator TwineVar(string val)
		{
			return new TwineVar(val);
		}

		public static implicit operator TwineVar(double val)
		{
			return new TwineVar(val);
		}

		public static implicit operator TwineVar(int val)
		{
			return new TwineVar(val);
		}

		public static implicit operator TwineVar(bool val)
		{
			return new TwineVar(val);
		}

		public static implicit operator string(TwineVar val)
		{
			return val.ConvertTo<string>();
		}

		public static implicit operator double(TwineVar val)
		{
			return val.ConvertTo<double>();
		}

		public static implicit operator int(TwineVar val)
		{
			return val.ConvertTo<int>();
		}

		public static implicit operator bool(TwineVar val)
		{
			return val.ConvertTo<bool>();
		}

		public static bool operator true(TwineVar val)
		{
			return val.ConvertTo<bool>();
		}

		public static bool operator false(TwineVar val)
		{
			return !val.ConvertTo<bool>();
		}
	}
}

