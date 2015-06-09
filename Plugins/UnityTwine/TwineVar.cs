using System;

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

		public static TwineVar operator++(TwineVar twee)
		{
			if (twee.value is int)
				twee.value = ((int)twee.value)+1;
			else if (twee.value is double)
				twee.value = ((double)twee.value)+1;

			return twee;
		}

		public static TwineVar operator--(TwineVar twee)
		{
			if (twee.value is int)
				twee.value = ((int)twee.value)-1;
			else if (twee.value is double)
				twee.value = ((double)twee.value)-1;

			return twee;
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

		public static implicit operator string(TwineVar twee) {
			return twee.ToString();
		}

		public static implicit operator TwineVar (string val) {
			return new TwineVar(val);
		}

		public static TwineVar operator+(TwineVar twee, string val) {
			return twee.ToString() + val;
		}

		public static bool operator==(TwineVar twee, string val) {
			return twee.ToString() == val;
		}

		public static bool operator!=(TwineVar twee, string val) {
			return twee.ToString() != val;
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
			if (this.value is int || this.value is double) {
				return (int) this.value;
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

		public static implicit operator int(TwineVar twee) {
			return twee.ToInt();
		}

		public static implicit operator TwineVar (int val) {
			return new TwineVar(val);
		}

		public static TwineVar operator+(TwineVar twee, int val) {
			if (twee.value is string) {
				return twee.ToString() + val.ToString();
			}
			else if (twee.value is int || twee.value is bool) {
				return twee.ToInt() + val;
			}
			else if (twee.value is double) {
				return twee.ToDouble() + (double) val;
			}
			else
				return twee;
		}

		public static TwineVar operator-(TwineVar twee, int val) {

			if (twee.value is double) {
				return twee.ToDouble() - (double) val;
			}
			else
				return twee.ToInt() - val;
		}

		public static TwineVar operator*(TwineVar twee, int val) {
			if (twee.value is double) {
				return twee.ToDouble() * (double) val;
			}
			else
				return twee.ToInt() * val;
		}

		public static TwineVar operator/(TwineVar twee, int val) {
			if (twee.value is double) {
				return twee.ToDouble() / (double) val;
			}
			else
				return twee.ToInt() / val;
		}

		public static TwineVar operator%(TwineVar twee, int val) {
			if (twee.value is double) {
				return twee.ToDouble() % (double) val;
			}
			else
				return twee.ToInt() % val;
		}

		public static bool operator==(TwineVar twee, int val) {
			return twee.ToInt() == val;
		}

		public static bool operator!=(TwineVar twee, int val) {
			return twee.ToInt() != val;
		}

		public static bool operator>(TwineVar twee, int val) {
			return twee.ToInt() > val;
		}

		public static bool operator<(TwineVar twee, int val) {
			return twee.ToInt() < val;
		}

		public static bool operator>=(TwineVar twee, int val) {
			return twee.ToInt() >= val;
		}

		public static bool operator<=(TwineVar twee, int val) {
			return twee.ToInt() <= val;
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

		public static implicit operator double(TwineVar twee) {
			return twee.ToDouble();
		}

		public static implicit operator TwineVar (double val) {
			return new TwineVar(val);
		}

		public static TwineVar operator+(TwineVar twee, double val) {
			if (twee.value is string) {
				return twee.ToString() + val.ToString();
			}
			else {
				return twee.ToDouble() + (double) val;
			}
		}

		public static TwineVar operator-(TwineVar twee, double val) {
			return twee.ToDouble() - val;
		}

		public static TwineVar operator*(TwineVar twee, double val) {
			return twee.ToDouble() * val;
		}

		public static TwineVar operator/(TwineVar twee, double val) {
			return twee.ToDouble() / val;
		}

		public static TwineVar operator%(TwineVar twee, double val) {
			return twee.ToDouble() % val;
		}

		public static bool operator==(TwineVar twee, double val) {
			return twee.ToDouble() == val;
		}

		public static bool operator!=(TwineVar twee, double val) {
			return twee.ToDouble() != val;
		}

		public static bool operator>(TwineVar twee, double val) {
			return twee.ToDouble() > val;
		}

		public static bool operator<(TwineVar twee, double val) {
			return twee.ToDouble() < val;
		}

		public static bool operator>=(TwineVar twee, double val) {
			return twee.ToDouble() >= val;
		}

		public static bool operator<=(TwineVar twee, double val) {
			return twee.ToDouble() <= val;
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

		public static implicit operator bool(TwineVar twee) {
			return twee.ToBool();
		}

		public static implicit operator TwineVar (bool val) {
			return new TwineVar(val);
		}

		public static bool operator true(TwineVar twee)
		{
			return twee.ToBool();
		}

		public static bool operator false(TwineVar twee)
		{
			return !twee.ToBool();
		}

		public static bool operator==(TwineVar twee, bool val) {
			return twee.ToBool() == val;
		}

		public static bool operator!=(TwineVar twee, bool val) {
			return twee.ToBool() != val;
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

