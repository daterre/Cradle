using System;
using System.Reflection;

namespace UnityTwine
{
	[Serializable]
	public struct TwineVar
	{
		#if UNITY_EDITOR
		private object _val;
		public object value
		{
			get { return _val; }
			set { _val = value; AsString = this.ToString(); }
		}
		public string AsString;
		#else
		private object value;
		#endif

		public Type GetInnerType()
		{
			return value == null ? null : value.GetType();
		}

		public TwineVar this[string propertyName]
		{
			get
			{
				object[] index;
				PropertyInfo property = GetProperty(propertyName, out index);

				if (!property.CanRead)
					throw new InvalidOperationException("Property '{0}' of this Twine variable is not readable.");

				object result;
				try { result = property.GetValue(this, index); }
				catch (Exception ex)
				{
					throw new InvalidOperationException("Error while trying to read property '{0}' of this Twine variable.", ex);
				}

				return new TwineVar(result);
			}
			set
			{
				object[] index;
				PropertyInfo property = GetProperty(propertyName, out index);

				if (!property.CanWrite)
					throw new InvalidOperationException("Property '{0}' of this Twine variable is not writable.");

				try { property.SetValue(this, value, index); }
				catch (Exception ex)
				{
					throw new InvalidOperationException("Error while trying to set property '{0}' of this Twine variable.", ex);
				}
			}
		}

		PropertyInfo GetProperty(string propertyName, out object[] index)
		{
			if (value == null)
				throw new InvalidOperationException("Cannot get property of empty Twine variable.");

			index = null;

			PropertyInfo property = value.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase);
			if (property == null)
			{
				// Try to use an indexer instead
				property = value.GetType().GetProperty("Item");
				if (property == null)
					throw new InvalidOperationException("Property '{0}' of this Twine variable could not be found.");

				index = new object[] { propertyName };
			}

			return property;
		}

		public override int GetHashCode()
		{
			if (value == null)
				return 0;

			int hash = 17;
			hash = hash * 31 + value.GetType().GetHashCode();
			hash = hash * 31 + value.GetHashCode();
			return hash;
		}

		public override bool Equals(object obj)
		{
			object val = obj is TwineVar ? ((TwineVar)obj).value : obj;

			if (val is bool)
				return this == (bool)val;
			else if (val is int)
				return this == (int)val;
			else if (val is double)
				return this == (double)val;
			else if (val is string)
				return this == (string)val;
			else
				return Object.Equals(this.value, obj);
		}

		public static TwineVar operator++(TwineVar twVar)
		{
			if (twVar.value is int)
				twVar.value = ((int)twVar.value)+1;
			else if (twVar.value is double)
				twVar.value = ((double)twVar.value)+1;

			return twVar;
		}

		public static TwineVar operator--(TwineVar twVar)
		{
			if (twVar.value is int)
				twVar.value = ((int)twVar.value)-1;
			else if (twVar.value is double)
				twVar.value = ((double)twVar.value)-1;

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
			if ((a.value is int || a.value is double || a.value is bool) && (b.value is int || b.value is double || b.value is bool))
				return a.ToDouble() + b.ToDouble();
			else
				return a.ToString() + b.ToString();
		}

		public static TwineVar operator -(TwineVar a, TwineVar b)
		{
			return a.ToDouble() - b.ToDouble();
		}

		// ..............
		// STRING

		public TwineVar(string val)
		{
			#if UNITY_EDITOR
			_val = val;
			AsString = ConvertToString(val);
			#else
			this.value = val;
			#endif
		}

		public override string ToString()
		{
			return ConvertToString(value);
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
			#if UNITY_EDITOR
			_val = val;
			AsString = ConvertToString(val);
			#else
			this.value = val;
			#endif
		}

		public int ToInt()
		{
			if (this.value is int) {
				return (int) this.value;
			}
			else if (this.value is double)
			{
				return Convert.ToInt32((double)this.value);
			}
			else if (this.value is string) {
				int parsed;
				if (int.TryParse((string)this.value, out parsed))
					return parsed;
				else
					return 0;
			}
			else if (this.value is bool ) {
				return ((bool)this.value) ? 1 : 0;
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
			if (twVar.value is string) {
				return twVar.ToString() + val.ToString();
			}
			else if (twVar.value is int || twVar.value is bool) {
				return twVar.ToInt() + val;
			}
			else if (twVar.value is double) {
				return twVar.ToDouble() + (double) val;
			}
			else
				return twVar;
		}

		public static TwineVar operator-(TwineVar twVar, int val) {

			if (twVar.value is double) {
				return twVar.ToDouble() - (double) val;
			}
			else
				return twVar.ToInt() - val;
		}

		public static TwineVar operator*(TwineVar twVar, int val) {
			if (twVar.value is double) {
				return twVar.ToDouble() * (double) val;
			}
			else
				return twVar.ToInt() * val;
		}

		public static TwineVar operator/(TwineVar twVar, int val) {
			if (twVar.value is double) {
				return twVar.ToDouble() / (double) val;
			}
			else
				return twVar.ToInt() / val;
		}

		public static TwineVar operator%(TwineVar twVar, int val) {
			if (twVar.value is double) {
				return twVar.ToDouble() % (double) val;
			}
			else
				return twVar.ToInt() % val;
		}

		public static bool operator==(TwineVar twVar, int val) {
			return twVar.ToInt() == val;
		}

		public static bool operator!=(TwineVar twVar, int val) {
			return twVar.ToInt() != val;
		}

		public static bool operator>(TwineVar twVar, int val) {
			return twVar.ToInt() > val;
		}

		public static bool operator<(TwineVar twVar, int val) {
			return twVar.ToInt() < val;
		}

		public static bool operator>=(TwineVar twVar, int val) {
			return twVar.ToInt() >= val;
		}

		public static bool operator<=(TwineVar twVar, int val) {
			return twVar.ToInt() <= val;
		}

		// ..............
		// DOUBLE

		public TwineVar(double val)
		{
			#if UNITY_EDITOR
			_val = val;
			AsString = ConvertToString(val);
			#else
			this.value = val;
			#endif
		}

		public double ToDouble()
		{
			if (this.value is double) {
				return (double) this.value;
			}
			else if (this.value is int) {
				return Convert.ToDouble((int)this.value);
			}
			else if (this.value is string) {
				double parsed;
				if (double.TryParse((string)this.value, out parsed))
					return parsed;
				else
					return 0.0;
			}
			else if (this.value is bool ) {
				return ((bool)this.value) ? 1.0 : 0.0;
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
			if (twVar.value is string) {
				return twVar.ToString() + val.ToString();
			}
			else {
				return twVar.ToDouble() + (double) val;
			}
		}

		public static TwineVar operator-(TwineVar twVar, double val) {
			return twVar.ToDouble() - val;
		}

		public static TwineVar operator*(TwineVar twVar, double val) {
			return twVar.ToDouble() * val;
		}

		public static TwineVar operator/(TwineVar twVar, double val) {
			return twVar.ToDouble() / val;
		}

		public static TwineVar operator%(TwineVar twVar, double val) {
			return twVar.ToDouble() % val;
		}

		public static bool operator==(TwineVar twVar, double val) {
			return twVar.ToDouble() == val;
		}

		public static bool operator!=(TwineVar twVar, double val) {
			return twVar.ToDouble() != val;
		}

		public static bool operator>(TwineVar twVar, double val) {
			return twVar.ToDouble() > val;
		}

		public static bool operator<(TwineVar twVar, double val) {
			return twVar.ToDouble() < val;
		}

		public static bool operator>=(TwineVar twVar, double val) {
			return twVar.ToDouble() >= val;
		}

		public static bool operator<=(TwineVar twVar, double val) {
			return twVar.ToDouble() <= val;
		}

		// ..............
		// BOOL

		public TwineVar(bool val)
		{
			#if UNITY_EDITOR
			_val = val;
			AsString = ConvertToString(val);
			#else
			this.value = val;
			#endif
		}

		public bool ToBool()
		{
			if (this.value is bool) {
				return (bool) this.value;
			}
			else if (this.value is int) {
				return ((int) this.value) != 0;
			}
			else if (this.value is double) {
				return ((double) this.value) != 0.0;
			}
			else if (this.value is string) {
				return ((string)this.value).Length > 0;
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

		// ..............
		// OBJECT

		public TwineVar(object val)
		{
			#if UNITY_EDITOR
			_val = val;
			AsString = ConvertToString(val);
			#else
			this.value = val;
			#endif
		}
	}
}

