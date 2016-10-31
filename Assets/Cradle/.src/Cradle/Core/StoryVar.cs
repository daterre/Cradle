using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Cradle
{
	[Serializable]
	public struct StoryVar
	{
		public static bool StrictMode = false;
		internal object Value;

		public struct MemberLookup
		{
			public string MemberName;

			public MemberLookup(string memberName)
			{
				MemberName = memberName;
			}

			public StoryVar this[StoryVar parent]
			{
				get { return parent.GetMember (MemberName); }
				set { parent.SetMember (MemberName, value); }
			}
		}

		public StoryVar(object value)
		{
			this.Value = GetInnerValue(value, true);
		}

		public Type InnerType
		{
			get
			{
				object val = GetInnerValue(this);
				return val == null ? null : val.GetType();
			}
		}

		public object InnerValue
		{
			get { return GetInnerValue(this); }
		}

		public static StoryVar Empty
		{
			get { return new StoryVar(null); }
		}

		private static object GetInnerValue(object obj, bool duplicate = false)
		{
			var twVar = default(StoryVar);
			while (obj is StoryVar)
			{
				twVar = (StoryVar)obj;
				obj = twVar.Value;
			}

			// When a duplicate is needed, duplicate only the last twine var in the chain
			if (duplicate && twVar.Value != null)
				obj = twVar.Duplicate().Value;

			return obj;
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
			return Compare(Operator.Equals, this, obj);
		}

		// ..............
		// TYPE SERVICES

		static Dictionary<Type, IVarTypeService> _typeServices = new Dictionary<Type, IVarTypeService>();

		public static void RegisterTypeService<T>(IVarTypeService service)
		{
			_typeServices[typeof(T)] = service;
		}

		public static VarTypeService<T> GetTypeService<T>(bool throwException = false)
		{
			var service = (VarTypeService<T>)GetTypeService(typeof(T));
			if (service == null)
				throw new StoryException(string.Format("UnityTwine is missing a TwineTypeService for {0}. Did you mess with something you shouldn't have?", typeof(T).FullName));

			return service;
		}

		public static IVarTypeService GetTypeService(Type t)
		{
			IVarTypeService service = null;
			_typeServices.TryGetValue(t, out service);
			return service;
		}

		// ..............
		// MEMBERS

		public StoryVar this[StoryVar memberName]
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

		public MemberLookup AsMemberOf
		{
			get { return new MemberLookup (this); }
		}

		public StoryVar GetMember(StoryVar member)
		{
			if (Value == null)
				throw new VarTypeMemberException("Cannot get members of an empty story variable.");

			if (member.Value == null)
				throw new VarTypeMemberException("Cannot treat an empty variable as a member.");

			IVarTypeService service = GetTypeService(this.Value.GetType());
			if (service != null)
				return service.GetMember(this.Value, member);

			if (this.Value is IVarType)
				return ((IVarType)this.Value).GetMember(member);

			throw new VarTypeMemberException(string.Format("Cannot get member of a story variable of type {0}.", this.Value.GetType().Name));
		}

		public void SetMember(StoryVar member, StoryVar val)
		{
			if (Value == null)
				throw new VarTypeMemberException("Cannot set member of empty story variable.");

			IVarTypeService service = GetTypeService(this.Value.GetType());
			if (service != null)
			{
				service.SetMember(this.Value, member, val);
				return;
			}

			if (this.Value is IVarType)
			{
				((IVarType)this.Value).SetMember(member, val);
				return;
			}

			throw new VarTypeMemberException(string.Format("Cannot set member of a story variable of type {0}.", this.Value.GetType().Name));
		}

		public void RemoveMember(StoryVar member)
		{
			if (Value == null)
				throw new VarTypeMemberException("Cannot remove member of empty story variable.");

			IVarTypeService service = GetTypeService(this.Value.GetType());
			if (service != null)
			{
				service.RemoveMember(this.Value, member);
				return;
			}

			if (this.Value is IVarType)
			{
				((IVarType)this.Value).RemoveMember(member);
				return;
			}

			throw new VarTypeMemberException(string.Format("Cannot remove member of a story variable of type {0}.", this.Value.GetType().Name));
		}

		// ..............
		// OBJECT

		public static bool Compare(Operator op, object left, object right)
		{
			object a = GetInnerValue(left);
			object b = GetInnerValue(right);

			if (a == null && b == null)
				return true;

			bool result;
			IVarTypeService service;

			if (a != null && _typeServices.TryGetValue(a.GetType(), out service) && service.Compare(op, a, b, out result))
				return result;

			if (a is IVarType)
			{
				if ((a as IVarType).Compare(op, b, out result))
					return result;
			}

			return false;
		}

		public static bool TryCombine(Operator op, object left, object right, out StoryVar result)
		{
			object a = GetInnerValue(left);
			object b = GetInnerValue(right);

			IVarTypeService service;

			if (a != null && _typeServices.TryGetValue(a.GetType(), out service) && service.Combine(op, a, b, out result))
				return true;

			if (a is IVarType)
			{
				if ((a as IVarType).Combine(op, b, out result))
					return true;
			}

			result = default(StoryVar);
			return false;
		}

		public static StoryVar Combine(Operator op, object left, object right)
		{
			object a = GetInnerValue(left);
			object b = GetInnerValue(right);

			StoryVar result;
			if (TryCombine(op, a, b, out result))
				return result;
			else
				throw new VarTypeException(string.Format("Cannot combine {0} with {1} using {2}",
					a == null ? "null" : a.GetType().Name,
					b == null ? "null" : b.GetType().Name,
					op
				));
		}

		public static StoryVar Unary(Operator op, object obj)
		{
			object a = GetInnerValue(obj);

			IVarTypeService service;

			StoryVar result;
			if (a != null && _typeServices.TryGetValue(a.GetType(), out service) && service.Unary(op, a, out result))
				return result;

			if (a is IVarType)
			{
				if ((a as IVarType).Unary(op, out result))
					return result;
			}

			throw new VarTypeException(string.Format("Cannot use {0} with {1}", op, a.GetType().Name ?? "null"));
		}

		public static bool TryConvertTo(object obj, Type t, out object result, bool strict)
		{
			object val = GetInnerValue(obj);

			// Source conversion
			if (val != null)
			{
				// Same type
				if (t.IsAssignableFrom(val.GetType()))
				{
					result = val;
					return true;
				}

				// Service type
				IVarTypeService service = GetTypeService(val.GetType());
				if (service != null && service.ConvertTo(val, t, out result, strict))
					return true;

				// Var type 
				if (val is IVarType)
				{
					if ((val as IVarType).ConvertTo(t, out result, strict))
						return true;
				}
			}
			
			// Target converion
			IVarTypeService targetService = GetTypeService(t);
			if (targetService != null && targetService.ConvertFrom(val, out result, strict))
				return true;

			result = null;
			return false;
		}

		public static bool TryConvertTo(object obj, Type t, out object result)
		{
			return TryConvertTo(obj, t, out result, StoryVar.StrictMode);
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
			return TryConvertTo<T>(obj, out result, StoryVar.StrictMode);
		}

		public static T ConvertTo<T>(object obj, bool strict)
		{
			obj = GetInnerValue(obj);

			T result;
			if (TryConvertTo<T>(obj, out result, strict))
				return result;
			else
				throw new VarTypeException(string.Format("Cannot convert {0} to {1}", obj == null ? "null" : obj.GetType().FullName, typeof(T).FullName));
		}

		public static T ConvertTo<T>(object obj)
		{
			return ConvertTo<T>(obj, StoryVar.StrictMode);
		}

		public StoryVar ConvertTo<T>()
		{
			return new StoryVar(StoryVar.ConvertTo<T>(this.Value));
		}

		public T ConvertValueTo<T>()
		{
			return StoryVar.ConvertTo<T>(this.Value);
		}

		public StoryVar Duplicate()
		{
			object val;
			if (this.Value == null || this.Value.GetType().IsValueType)
			{
				val = this.Value;
			}
			else
			{
				// Service type
				IVarTypeService service = GetTypeService(this.Value.GetType());
				if (service != null)
					val = service.Duplicate(this.Value);

				// Var type 
				else if (this.Value is IVarType)
					val = (this.Value as IVarType).Duplicate();

				val = this.Value;
			}

			return new StoryVar(val);
		}

		public bool Contains(object obj)
		{
			return Compare(Operator.Contains, this, obj);
		}

		public bool ContainedBy(object obj)
		{
			return Compare(Operator.Contains, obj, this);
		}

		public void PutInto(ref StoryVar varRef)
		{
			varRef = this;
		}

		#region Operators
		// ------------------------

		public static StoryVar operator++(StoryVar val)
		{
			return Unary(Operator.Increment, val.Value);
		}

		public static StoryVar operator--(StoryVar val)
		{
			return Unary(Operator.Decrement, val.Value);
		}

		public static bool operator==(StoryVar a, object b)
		{
			return Compare(Operator.Equals, a, b);
		}

		public static bool operator!=(StoryVar a, object b)
		{
			return !(a == b);
		}

		public static bool operator >(StoryVar a, object b)
		{
			return Compare(Operator.GreaterThan, a, b);
		}

		public static bool operator >=(StoryVar a, object b)
		{
			return Compare(Operator.GreaterThanOrEquals, a, b);
		}

		public static bool operator <(StoryVar a, object b)
		{
			return Compare(Operator.LessThan, a, b);
		}

		public static bool operator <=(StoryVar a, object b)
		{
			return Compare(Operator.LessThanOrEquals, a, b);
		}

		public static StoryVar operator +(StoryVar a, object b)
		{
			return Combine(Operator.Add, a, b);
		}

		public static StoryVar operator -(StoryVar a, object b)
		{
			return Combine(Operator.Subtract, a, b);
		}

		public static StoryVar operator *(StoryVar a, object b)
		{
			return Combine(Operator.Multiply, a, b);
		}

		public static StoryVar operator /(StoryVar a, object b)
		{
			return Combine(Operator.Divide, a, b);
		}

		public static StoryVar operator %(StoryVar a, object b)
		{
			return Combine(Operator.Modulo, a, b);
		}

		public static StoryVar operator &(StoryVar a, StoryVar b)
		{
			return Combine(Operator.LogicalAnd, a, b);
		}

		public static StoryVar operator |(StoryVar a, StoryVar b)
		{
			return Combine(Operator.LogicalOr, a, b);
		}

		public static implicit operator StoryVar(string val)
		{
			return new StoryVar(val);
		}

		public static implicit operator StoryVar(double val)
		{
			return new StoryVar(val);
		}

		public static implicit operator StoryVar(int val)
		{
			return new StoryVar(val);
		}

		public static implicit operator StoryVar(bool val)
		{
			return new StoryVar(val);
		}

		public static implicit operator StoryVar(VarType val)
		{
			return new StoryVar(val);
		}

		public static implicit operator string(StoryVar val)
		{
			return ConvertTo<string>(val);
		}

		public static implicit operator double(StoryVar val)
		{
			return ConvertTo<double>(val);
		}

		public static implicit operator int(StoryVar val)
		{
			return ConvertTo<int>(val);
		}

		public static implicit operator bool(StoryVar val)
		{
			return ConvertTo<bool>(val);
		}

		public static bool operator true(StoryVar val)
		{
			return ConvertTo<bool>(val);
		}

		public static bool operator false(StoryVar val)
		{
			return !ConvertTo<bool>(val);
		}
		// ------------------------
		#endregion
	}
}

