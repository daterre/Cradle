using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using List = System.Collections.Generic.List<object>;

namespace UnityTwine
{
	/// <summary>
	/// An immutable type that represents settings for a scope. Combine settings with the + operator.
	/// </summary>
	public class TwineStyle: TwineType
	{
		public const string EvaluatedValContextOption = "evaluated-expression";

		Dictionary<string, List> _settings = new Dictionary<string, List>();

		public TwineStyle()
		{
		}

		public TwineStyle(string name, object value)
		{
			Set(name, value);
		}

		public TwineStyle(TwineVar val)
		{
			Set(val);
		}

		public static implicit operator TwineStyle(TwineVar styleVar)
		{
			return styleVar.ConvertValueTo<TwineStyle>();
		}

		public static bool operator true(TwineStyle style)
		{
			return style != null && style._settings.Count > 0;
		}

		public static bool operator false(TwineStyle style)
		{
			return style == null || style._settings.Count == 0;
		}

		public static TwineStyle operator+(TwineStyle a, TwineStyle b)
		{
			return TwineVar.Combine(TwineOperator.Add, a, b).ConvertValueTo<TwineStyle>();
		}

		public List<T> GetValues<T>(string setting)
		{
			List values;
			if (!_settings.TryGetValue(setting, out values))
				return new List<T>();
			else
				return new List<T>(values.Select(obj => (T)obj));
		}

		public List GetValues(string name)
		{
			return GetValues<object>(name);
		}

		public Dictionary<string,List>.KeyCollection SettingNames
		{
			get { return _settings.Keys; }
		}

		void Set(string name, object value)
		{
			List values;
			if (!_settings.TryGetValue(name, out values))
				_settings[name] = values = new List();

			if (!values.Contains(value))
				values.Add(value);
		}

		void Set(TwineVar booleanExpression)
		{
			if (booleanExpression.ConvertValueTo<bool>())
				Set(EvaluatedValContextOption, booleanExpression);
		}

		//void Recalculate()
		//{
		//	_calculatedValues.Clear();
		//	for (int i = 0; i < _appliedContexts.Count; i++)
		//	{
		//		TwineContext context = _appliedContexts[i];
		//		foreach (var entry in context._calculatedValues)
		//			RecalculateEntry(entry);
		//	}

		//	foreach (var entry in this._settings)
		//		RecalculateEntry(entry);
		//}

		//void RecalculateEntry(KeyValuePair<string, List> entry)
		//{
		//	List values;
		//	if (!_calculatedValues.TryGetValue(entry.Key, out values))
		//		_calculatedValues[entry.Key] = values = new List();
		//	values.AddRange(entry.Value);
		//}

		public TwineStyle GetCopy()
		{
			var copy = new TwineStyle()
			{
				_settings = new Dictionary<string, List>()
			};

			foreach (var entry in this._settings)
				copy._settings[entry.Key] = new List(entry.Value);

			return copy;
		}

		public override TwineVar GetMember(TwineVar member)
		{
			throw new NotImplementedException();
		}

		public override void SetMember(TwineVar member, TwineVar value)
		{
			throw new NotImplementedException();
		}

		public override void RemoveMember(TwineVar member)
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
			var bStyle = (TwineStyle)b;

			var combined = this.GetCopy();
			foreach (string setting in bStyle.SettingNames)
				foreach (object value in bStyle.GetValues(setting))
					combined.Set(setting, value);

			result = combined;
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

		public override ITwineType Duplicate()
		{
			return this.GetCopy();
		}
	}
}
