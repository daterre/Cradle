using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace UnityTwine
{
	public abstract class TwineRuntimeVars: IDictionary<string, TwineVar>
	{
		public TwineStory Story;
		public bool StrictMode;

		protected class Accessor
		{
			public Func<TwineVar> Get;
			public Action<TwineVar> Set;
		}

		protected Dictionary<string, Accessor> accessors = new Dictionary<string, Accessor>();

		protected void VarDef(string name, Func<TwineVar> getter, Action<TwineVar> setter)
		{
			accessors.Add(name, new Accessor() { Get = getter, Set = setter});
		}

		internal void Reset()
		{
			foreach (Accessor accessor in accessors.Values)
				accessor.Set(default(TwineVar));
		}

		public bool ContainsKey(string varName)
		{
			return accessors.ContainsKey(varName);
		}

		public ICollection<string> Keys
		{
			get { return accessors.Keys; }
		}

		public bool TryGetValue(string varName, out TwineVar value)
		{
			value = default(TwineVar);
			Accessor accessor;
			if (accessors.TryGetValue(varName, out accessor))
				return false;

			return accessor.Get();
		}

		public ICollection<TwineVar> Values
		{
			get { return accessors.Values.Select(accessor => accessor.Get()).ToArray(); }
		}

		public TwineVar this[string varName]
		{
			get
			{
				return GetMember(varName);
			}
			set
			{
				SetMember(varName, value);
			}
		}

		public TwineVar GetMember(string varName)
		{
			var value = default(TwineVar);
			if (!TryGetValue(varName, out value))
				throw new TwineException(string.Format("There is no variable with the name '{0}'.", varName));
			return value;
		}

		public void SetMember(string varName, TwineVar value)
		{
			TwineVar prevValue = this[varName];
			object v = value.Clone().Value;

			// Enfore strict mode
			if (StrictMode)
			{
				if (prevValue.Value != null && !TwineVar.TryConvertTo(v, prevValue.GetInnerType(), out v))
					throw new TwineStrictModeException(string.Format("The variable '{0}' was previously assigned a value of type {1}, and so cannot be assigned a value of type {2}.",
						varName,
						prevValue.Value.GetType().Name,
						v == null ? "null" : v.GetType().Name
					));
			}

			// Run the setter
			accessors[varName].Set(new TwineVar(v));
		}

		public int Count
		{
			get { return accessors.Count; }
		}

		void IDictionary<string, TwineVar>.Add(string key, TwineVar value)
		{
			throw new NotSupportedException();
		}

		bool IDictionary<string, TwineVar>.Remove(string key)
		{
			throw new NotSupportedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		void ICollection<KeyValuePair<string, TwineVar>>.Add(KeyValuePair<string, TwineVar> item)
		{
			throw new NotSupportedException();
		}

		void ICollection<KeyValuePair<string, TwineVar>>.Clear()
		{
			throw new NotSupportedException();
		}

		bool ICollection<KeyValuePair<string, TwineVar>>.Contains(KeyValuePair<string, TwineVar> item)
		{
			return ((ICollection<KeyValuePair<string, TwineVar>>)accessors).Contains(item);
		}

		void ICollection<KeyValuePair<string, TwineVar>>.CopyTo(KeyValuePair<string, TwineVar>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<string, TwineVar>>)accessors).CopyTo(array, arrayIndex);
		}

		bool ICollection<KeyValuePair<string, TwineVar>>.IsReadOnly
		{
			get { return ((ICollection<KeyValuePair<string, TwineVar>>)accessors).IsReadOnly; }
		}

		bool ICollection<KeyValuePair<string, TwineVar>>.Remove(KeyValuePair<string, TwineVar> item)
		{
			throw new NotSupportedException();
		}

		IEnumerator<KeyValuePair<string, TwineVar>> IEnumerable<KeyValuePair<string, TwineVar>>.GetEnumerator()
		{
			return accessors.Select(pair => new KeyValuePair<string, TwineVar>(pair.Key, pair.Value.Get())).GetEnumerator();
		}

		TwineVar IDictionary<string, TwineVar>.this[string key]
		{
			get
			{
				return GetMember(key);
			}
			set
			{
				SetMember(key, value);
			}
		}
	}
}

