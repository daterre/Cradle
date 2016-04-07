using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityTwine
{
	public class TwineStyle: TwineType, IDisposable
	{
		public event Action<TwineStyle> OnDisposed;
		public event Action<TwineStyle> OnChanged;
		public bool IsReadOnly { get; private set; }

		List<TwineStyle> _appliedStyles = new List<TwineStyle>();
		Dictionary<string, object> _appliedValues = new Dictionary<string, object>();
		Dictionary<string, object> _calculatedValues = new Dictionary<string, object>();

		public TwineStyle()
		{
		}

		public TwineStyle(string name, object value)
		{
			_appliedValues[name] = value;
		}

		public object this[string name]
		{
			get
			{
				object val;
				if (_calculatedValues.TryGetValue(name, out val))
					return val;
				else
					return null;
			}
			set
			{
				if (IsReadOnly)
					throw new TwineException("This style object is a copy and cannot be modified.");

				_appliedValues[name] = value;
				Recalculate();
			}
		}

		public T Get<T>(string name)
		{
			object val = this[name];
			return val == null ? default(T) : (T)val;
		}


		public object GetApplied(string name)
		{
			object val;
			if (_appliedValues.TryGetValue(name, out val))
				return val;
			else
				return null;
		}

		public void Apply(TwineStyle style)
		{
			if (IsReadOnly)
				throw new TwineException("This style object is a copy and cannot be modified.");

			if (_appliedStyles.Contains(style))
				throw new InvalidOperationException("This style object has already been applied.");

			if (style._appliedStyles.Count > 0)
				throw new InvalidOperationException("Cannot apply a style that has its own applied styles.");

			style.OnDisposed += Unapply;
			_appliedStyles.Add(style);

			Recalculate();
		}

		public TwineStyle Apply(string key, object value)
		{
			var newStyle = new TwineStyle();
			newStyle[key] = value;
			Apply(newStyle);
			return newStyle;
		}

		public void Unapply(TwineStyle style)
		{
			if (!_appliedStyles.Contains(style))
				return;

			style.OnDisposed -= Unapply;
			_appliedStyles.Remove(style);
			
			Recalculate();
		}

		void Recalculate()
		{
			_calculatedValues.Clear();
			for (int i = 0; i < _appliedStyles.Count; i++)
			{
				TwineStyle style = _appliedStyles[i];
				foreach (var entry in style._appliedValues)
					_calculatedValues[entry.Key] = entry.Value;
			}

			foreach (var entry in this._appliedValues)
				_calculatedValues[entry.Key] = entry.Value;
		}

		public void Dispose()
		{
			if (this.OnDisposed != null)
				this.OnDisposed(this);
		}

		public TwineStyle GetCopy()
		{
			var copy = new TwineStyle();
			copy._calculatedValues = new Dictionary<string, object>(this._calculatedValues);
			copy.IsReadOnly = true;
			return copy;
		}

		public override TwineVar GetMember(string memberName)
		{
			throw new NotImplementedException();
		}

		public override void SetMember(string memberName, TwineVar value)
		{
			throw new NotImplementedException();
		}

		public override void RemoveMember(string memberName)
		{
			throw new NotImplementedException();
		}

		public override bool Compare(TwineOperator op, object b, out bool result)
		{
			result = default(TwineVar);
			return false;
		}

		public override bool Combine(TwineOperator op, object b, out TwineVar result)
		{
			result = default(TwineVar);
			if (!(b is TwineStyle) || op != TwineOperator.Add)
				return false;
			TwineStyle bStyle = (TwineStyle)b;

			var combinedStyle = new TwineStyle();
			combinedStyle.Apply(this);
			combinedStyle.Apply(bStyle);
			result = combinedStyle.GetCopy();
			return true;
		}

		public override bool Unary(TwineOperator op, out TwineVar result)
		{
			result = default(TwineVar);
			return false;
		}

		public override bool ConvertTo(Type t, out object result, bool strict = false)
		{
			result = null;
			return false;
		}
	}
}
