using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnityTwine
{
	[Serializable]
	public struct TwineVar
	{
		public static bool StrictMode = false;

		internal object Container;
		internal string Member;
		internal object Value;

		public TwineVar(object value): this(null,null,value)
		{
		}

		public TwineVar(object container, string member):this(container, member, null)
		{
		}

		public TwineVar(object container, string member, object value)
		{
			Container = container;
			Member = member;
			this.Value = value is TwineVar ? ((TwineVar)value).Value : value;
		}

		public Type GetInnerType()
		{
			return Value == null ? null : Value.GetType();
		}

		internal TwineVar ContainerVar
		{
			get { return Container is TwineVar ? (TwineVar) Container : new TwineVar(Container); }
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

		public override bool Equals(object obj)
		{
			return Compare(TwineOperator.Equals, this, obj);
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

		public TwineVar this[TwineVar memberName]
		{
			get
			{
				return GetMember(memberName);
			}
			set
			{
				SetMember(memberName, value);
			}
		}

		public TwineVar AsMemberOf(TwineVar val)
		{
			return val.GetMember(this);
		}

		public TwineVar GetMember(string memberName)
		{
			if (Value == null)
				throw new TwineTypeMemberException("Cannot get member of empty Twine var.");

			ITwineTypeService service = GetTypeService(this.Value.GetType());
			if (service != null)
				return service.GetMember(this.Value, memberName);

			if (this.Value is ITwineType)
				return ((ITwineType)this.Value).GetMember(memberName);

			throw new TwineTypeMemberException(string.Format("Cannot get member of a Twine var of type {0}.", this.Value.GetType().Name));
		}

		public TwineVar GetMember(TwineVar memberName)
		{
			return GetMember(memberName.ToString());
		}

		public void SetMember(string memberName, TwineVar val)
		{
			if (Value == null)
				throw new TwineTypeMemberException("Cannot set member of empty Twine var.");

			ITwineTypeService service = GetTypeService(this.Value.GetType());
			if (service != null)
			{
				service.SetMember(this.Value, memberName, val);
				return;
			}

			if (this.Value is ITwineType)
			{
				((ITwineType)this.Value).SetMember(memberName, val);
				return;
			}

			throw new TwineTypeMemberException(string.Format("Cannot set member of a Twine var of type {0}.", this.Value.GetType().Name));
		}

		public void SetMember(TwineVar memberName, TwineVar val)
		{
			SetMember(memberName.ToString(), val);
		}

		public void RemoveMember(string memberName)
		{
			if (Value == null)
				throw new TwineTypeMemberException("Cannot remove member of empty Twine var.");

			ITwineTypeService service = GetTypeService(this.Value.GetType());
			if (service != null)
			{
				service.RemoveMember(this.Value, memberName);
				return;
			}

			if (this.Value is ITwineType)
			{
				((ITwineType)this.Value).RemoveMember(memberName);
				return;
			}

			throw new TwineTypeMemberException(string.Format("Cannot remove member of a Twine var of type {0}.", this.Value.GetType().Name));
		}

		public void RemoveMember(TwineVar memberName)
		{
			RemoveMember(memberName.ToString());
		}

		// ..............
		// OBJECT


		public static bool Compare(TwineOperator op, object left, object right)
		{
			object a = left is TwineVar ? ((TwineVar)left).Value : left;
			object b = right is TwineVar ? ((TwineVar)right).Value : right;

			bool result;
			ITwineTypeService service;

			Type t = typeof(void);

			// TODO: null service?

			if (a != null && _typeServices.TryGetValue(a.GetType(), out service) && service.Compare(op, a, b, out result))
				return result;

			if (a is ITwineType)
			{
				if ((a as ITwineType).Compare(op, b, out result))
					return result;
			}

			//if (b != null && _typeServices.TryGetValue(b.GetType(), out service) && service.Compare(op, a, b, out result))
			//	return result;

			//if (b is ITwineType)
			//{
			//	if ((b as ITwineType).Compare(op, a, out result))
			//		return result;
			//}

			return false;
		}

		public static bool TryCombine(TwineOperator op, object left, object right, out TwineVar result)
		{
			object a = left is TwineVar ? ((TwineVar)left).Value : left;
			object b = right is TwineVar ? ((TwineVar)right).Value : right;

			ITwineTypeService service;

			if (a != null && _typeServices.TryGetValue(a.GetType(), out service) && service.Combine(op, a, b, out result))
				return true;

			if (a is ITwineType)
			{
				if ((a as ITwineType).Combine(op, b, out result))
					return true;
			}

			//if (b != null && _typeServices.TryGetValue(b.GetType(), out service) && service.Combine(op, a, b, out result))
			//	return true;

			//if (b is ITwineType)
			//{
			//	if ((b as ITwineType).Combine(op, a, out result))
			//		return true;
			//}

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

		public static bool TryConvertTo(object obj, Type t, out object result, bool strict)
		{
			object val = obj is TwineVar ? ((TwineVar)obj).Value : obj;

			// Same type
			if (val != null && t.IsAssignableFrom(val.GetType()))
			{
				result = val;
				return true;
			}

			ITwineTypeService service = val == null ? null : GetTypeService(val.GetType());
			if (service != null && service.ConvertTo(val, t, out result, strict))
				return true;

			if (val is ITwineType)
			{
				if ((val as ITwineType).ConvertTo(t, out result, strict))
					return true;
			}

			result = null;
			return false;
		}

		public static bool TryConvertTo(object obj, Type t, out object result)
		{
			return TryConvertTo(obj, t, out result, TwineVar.StrictMode);
		}

		public static bool TryConvertTo<T>(object obj, out T result, bool strict)
		{
			object r;
			if (TryConvertTo(obj, typeof(T), out r, strict))
			{
				result = (T)r;
				return true;
			}
			else
			{
				result = default(T);
				return false;
			}
		}

		public static bool TryConvertTo<T>(object obj, out T result)
		{
			return TryConvertTo<T>(obj, out result, TwineVar.StrictMode);
		}

		public static T ConvertTo<T>(object obj, bool strict)
		{
			T result;
			if (TryConvertTo<T>(obj, out result, strict))
				return result;
			else
				throw new TwineTypeException(string.Format("Cannot convert {0} to {1}", obj == null ? "null" : obj.GetType().FullName, typeof(T).FullName));
		}

		public static T ConvertTo<T>(object obj)
		{
			return ConvertTo<T>(obj, TwineVar.StrictMode);
		}

		public bool Contains(object obj)
		{
			return Compare(TwineOperator.Contains, this, obj);
		}

		public bool ContainedBy(object obj)
		{
			return Compare(TwineOperator.Contains, obj, this);
		}

		public void ReplaceWith(TwineVar val)
		{
			if (this.ContainerVar.Value != null)
				this.ContainerVar.SetMember(this.Member, val);
			this.Value = val;
		}

		public void PutInto(TwineVar varRef)
		{
			varRef.ReplaceWith(this);
		}

		public void MoveInto(TwineVar varRef)
		{
			varRef.ReplaceWith(this);

			if (this.ContainerVar.Value != null)
				this.ContainerVar.RemoveMember(this.Member);

			this.Container = null;
			this.Member = null;
			this.Value = default(TwineVar);
		}

		#region Operators
		// ------------------------

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

		public static implicit operator TwineVar(TwineType val)
		{
			return new TwineVar(val);
		}

		public static implicit operator string(TwineVar val)
		{
			return ConvertTo<string>(val);
		}

		public static implicit operator double(TwineVar val)
		{
			return ConvertTo<double>(val);
		}

		public static implicit operator int(TwineVar val)
		{
			return ConvertTo<int>(val);
		}

		public static implicit operator bool(TwineVar val)
		{
			return ConvertTo<bool>(val);
		}

		public static bool operator true(TwineVar val)
		{
			return ConvertTo<bool>(val);
		}

		public static bool operator false(TwineVar val)
		{
			return ConvertTo<bool>(val);
		}
		// ------------------------
		#endregion
	}
}

