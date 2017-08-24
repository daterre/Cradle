using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cradle
{
	public class Style: VarType, IDictionary<string, object>
	{
		public const string EvaluatedValContextOption = "evaluated-expression";

		Dictionary<string, object> _entries = new Dictionary<string, object>();

		public Style()
		{
		}

		public Style(string name, object value)
		{
			this[name] = value;
		}

		public Style(StoryVar booleanExpression)
		{
			// When giving a single value
			if (booleanExpression.ConvertValueTo<bool>())
				this[EvaluatedValContextOption] = booleanExpression;
		}

		public static implicit operator Style(StoryVar styleVar)
		{
			return styleVar.ConvertValueTo<Style>();
		}

		public static bool operator true(Style style)
		{
			return style != null && style._entries.Count > 0;
		}

		public static bool operator false(Style style)
		{
			return style == null || style._entries.Count == 0;
		}

		public static Style operator+(Style a, Style b)
		{
			return StoryVar.Combine(Operator.Add, a, b).ConvertValueTo<Style>();
		}

		public object this[string key]
		{
			get
			{
				object val = null;
				_entries.TryGetValue(key, out val);
				return val;
			}
			set { _entries[key] = value; }
		}

		public T Get<T>(string key)
		{
			object val = null;
			if (_entries.TryGetValue(key, out val))
				return (T)val;
			else
				return default(T);
		}

		public Style GetCopy()
		{
			var copy = new Style()
			{
				_entries = new Dictionary<string, object>(this._entries)
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
			_entries.Remove(member);
		}

		public override bool Compare(Operator op, object b, out bool result)
		{
			result = default(StoryVar);
			return false;
		}

		public override bool Combine(Operator op, object b, out StoryVar result)
		{
			result = default(StoryVar);
			if (!(b is Style) || op != Operator.Add)
				return false;
			var bStyle = (Style)b;

			var combined = this.GetCopy();
			foreach (var entry in bStyle._entries)
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
			_entries.Add(key, value);
		}

		public bool ContainsKey(string key)
		{
			return _entries.ContainsKey(key);
		}

		public ICollection<string> Keys
		{
			get { return _entries.Keys; }
		}

		public bool Remove(string key)
		{
			return _entries.Remove(key);
		}

		bool IDictionary<string, object>.TryGetValue(string key, out object value)
		{
			return _entries.TryGetValue(key, out value);
		}

		public ICollection<object> Values
		{
			get { return _entries.Values; }
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ((System.Collections.IEnumerable)_entries).GetEnumerator();
		}

		void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
		{
			((ICollection<KeyValuePair<string, object>>)_entries).Add(item);
		}

		public void Clear()
		{
			_entries.Clear();
		}

		bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
		{
			return ((ICollection<KeyValuePair<string, object>>)_entries).Contains(item);
		}

		void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<string, object>>)_entries).CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return _entries.Count; }
		}

		bool ICollection<KeyValuePair<string, object>>.IsReadOnly
		{
			get { return ((ICollection<KeyValuePair<string, object>>)_entries).IsReadOnly; }
		}

		bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
		{
			return ((ICollection<KeyValuePair<string, object>>)_entries).Remove(item);
		}

		IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<string, object>>)_entries).GetEnumerator();
		}
	}
}
