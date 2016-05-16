using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using List = System.Collections.Generic.List<object>;

namespace Cradle
{
	/// <summary>
	/// An immutable type that represents settings for a scope. Combine settings with the + operator.
	/// </summary>
	public class StoryStyle: VarType
	{
		public const string EvaluatedValContextOption = "evaluated-expression";

		Dictionary<string, List> _settings = new Dictionary<string, List>();

		public StoryStyle()
		{
		}

		public StoryStyle(string name, object value)
		{
			Set(name, value);
		}

		public StoryStyle(StoryVar val)
		{
			Set(val);
		}

		public static implicit operator StoryStyle(StoryVar styleVar)
		{
			return styleVar.ConvertValueTo<StoryStyle>();
		}

		public static bool operator true(StoryStyle style)
		{
			return style != null && style._settings.Count > 0;
		}

		public static bool operator false(StoryStyle style)
		{
			return style == null || style._settings.Count == 0;
		}

		public static StoryStyle operator+(StoryStyle a, StoryStyle b)
		{
			return StoryVar.Combine(Operator.Add, a, b).ConvertValueTo<StoryStyle>();
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

		void Set(StoryVar booleanExpression)
		{
			if (booleanExpression.ConvertValueTo<bool>())
				Set(EvaluatedValContextOption, booleanExpression);
		}

		public StoryStyle GetCopy()
		{
			var copy = new StoryStyle()
			{
				_settings = new Dictionary<string, List>()
			};

			foreach (var entry in this._settings)
				copy._settings[entry.Key] = new List(entry.Value);

			return copy;
		}

		public override StoryVar GetMember(StoryVar member)
		{
			throw new NotImplementedException();
		}

		public override void SetMember(StoryVar member, StoryVar value)
		{
			throw new NotImplementedException();
		}

		public override void RemoveMember(StoryVar member)
		{
			throw new NotImplementedException();
		}

		public override bool Compare(Operator op, object b, out bool result)
		{
			result = default(StoryVar);
			return false;
		}

		public override bool Combine(Operator op, object b, out StoryVar result)
		{
			result = default(StoryVar);
			if (!(b is StoryStyle) || op != Operator.Add)
				return false;
			var bStyle = (StoryStyle)b;

			var combined = this.GetCopy();
			foreach (string setting in bStyle.SettingNames)
				foreach (object value in bStyle.GetValues(setting))
					combined.Set(setting, value);

			result = combined;
			return true;
		}

		public override bool Unary(Operator op, out StoryVar result)
		{
			result = default(StoryVar);
			return false;
		}

		public override bool ConvertTo(Type t, out object result, bool strict = false)
		{
			result = null;
			return false;
		}

		public override IVarType Duplicate()
		{
			return this.GetCopy();
		}
	}
}
