using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cradle
{
	public class StoryStyle: VarType, IDictionary<string, object>
	{
		public const string EvaluatedValContextOption = "evaluated-expression";

		Dictionary<string, object> _settings = new Dictionary<string, object>();

		public StoryStyle()
		{
		}

		public StoryStyle(string name, object value)
		{
			this[name] = value;
		}

		public StoryStyle(StoryVar booleanExpression)
		{
			// When giving a single value
			if (booleanExpression.ConvertValueTo<bool>())
				this[EvaluatedValContextOption] = booleanExpression;
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

		public object this[string setting]
		{
			get
			{
				object val = null;
				_settings.TryGetValue(setting, out val);
				return val;
			}
			set { _settings[setting] = value; }
		}

		public T Get<T>(string setting)
		{
			object val = null;
			if (_settings.TryGetValue(setting, out val))
				return (T)val;
			else
				return default(T);
		}

		public StoryStyle GetCopy()
		{
			var copy = new StoryStyle()
			{
				_settings = new Dictionary<string, object>(this._settings)
			};

			return copy;
		}

		public override StoryVar GetMember(StoryVar member)
		{
			return new StoryVar(this[member]);
		}

		public override void SetMember(StoryVar member, StoryVar value)
		{
			this[member] = value;
		}

		public override void RemoveMember(StoryVar member)
		{
			_settings.Remove(member);
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
			foreach (var entry in bStyle._settings)
				combined[entry.Key] = entry.Value;

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

		public void Add(string key, object value)
		{
			_settings.Add(key, value);
		}

		public bool ContainsKey(string key)
		{
			return _settings.ContainsKey(key);
		}

		public ICollection<string> Keys
		{
			get { return _settings.Keys; }
		}

		public bool Remove(string key)
		{
			return _settings.Remove(key);
		}

		bool IDictionary<string, object>.TryGetValue(string key, out object value)
		{
			return _settings.TryGetValue(key, out value);
		}

		public ICollection<object> Values
		{
			get { return _settings.Values; }
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ((System.Collections.IEnumerable)_settings).GetEnumerator();
		}

		void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
		{
			((ICollection<KeyValuePair<string, object>>)_settings).Add(item);
		}

		public void Clear()
		{
			_settings.Clear();
		}

		bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
		{
			return ((ICollection<KeyValuePair<string, object>>)_settings).Contains(item);
		}

		void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<string, object>>)_settings).CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return _settings.Count; }
		}

		bool ICollection<KeyValuePair<string, object>>.IsReadOnly
		{
			get { return ((ICollection<KeyValuePair<string, object>>)_settings).IsReadOnly; }
		}

		bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
		{
			return ((ICollection<KeyValuePair<string, object>>)_settings).Remove(item);
		}

		IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<string, object>>)_settings).GetEnumerator();
		}
	}
}
