using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Cradle
{
	public abstract class RuntimeVars: IDictionary<string, StoryVar>
	{
		public Story Story;
		public bool StrictMode;

		protected class Accessor
		{
			public Func<StoryVar> Get;
			public Action<StoryVar> Set;
		}

		protected Dictionary<string, Accessor> accessors = new Dictionary<string, Accessor>();

		protected void VarDef(string name, Func<StoryVar> getter, Action<StoryVar> setter)
		{
			accessors.Add(name, new Accessor() { Get = getter, Set = setter});
		}

		internal void Reset()
		{
			foreach (Accessor accessor in accessors.Values)
				accessor.Set(default(StoryVar));
		}

		public bool ContainsKey(string varName)
		{
			return accessors.ContainsKey(varName);
		}

		public ICollection<string> Keys
		{
			get { return accessors.Keys; }
		}

		public bool TryGetValue(string varName, out StoryVar value)
		{
			value = default(StoryVar);
			Accessor accessor;
			if (!accessors.TryGetValue(varName, out accessor))
				return false;

			value = accessor.Get();
			return true;
		}

		public ICollection<StoryVar> Values
		{
			get { return accessors.Values.Select(accessor => accessor.Get()).ToArray(); }
		}

		public StoryVar this[string varName]
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

		public StoryVar GetMember(string varName)
		{
			var value = default(StoryVar);
			if (!TryGetValue(varName, out value))
				throw new StoryException(string.Format("There is no variable with the name '{0}'.", varName));
			return value;
		}

		public void SetMember(string varName, StoryVar value)
		{
			StoryVar prevValue = this[varName];
			object v = value.Duplicate().Value;

			// Enfore strict mode
			if (StrictMode)
			{
				if (prevValue.Value != null && !StoryVar.TryConvertTo(v, prevValue.InnerType, out v))
					throw new StrictModeException(string.Format("The variable '{0}' was previously assigned a value of type {1}, and so cannot be assigned a value of type {2}.",
						varName,
						prevValue.Value.GetType().Name,
						v == null ? "null" : v.GetType().Name
					));
			}

			// Run the setter
			accessors[varName].Set(new StoryVar(v));
		}

		public int Count
		{
			get { return accessors.Count; }
		}

		void IDictionary<string, StoryVar>.Add(string key, StoryVar value)
		{
			throw new NotSupportedException();
		}

		bool IDictionary<string, StoryVar>.Remove(string key)
		{
			throw new NotSupportedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		void ICollection<KeyValuePair<string, StoryVar>>.Add(KeyValuePair<string, StoryVar> item)
		{
			throw new NotSupportedException();
		}

		void ICollection<KeyValuePair<string, StoryVar>>.Clear()
		{
			throw new NotSupportedException();
		}

		bool ICollection<KeyValuePair<string, StoryVar>>.Contains(KeyValuePair<string, StoryVar> item)
		{
			return ((ICollection<KeyValuePair<string, StoryVar>>)accessors).Contains(item);
		}

		void ICollection<KeyValuePair<string, StoryVar>>.CopyTo(KeyValuePair<string, StoryVar>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<string, StoryVar>>)accessors).CopyTo(array, arrayIndex);
		}

		bool ICollection<KeyValuePair<string, StoryVar>>.IsReadOnly
		{
			get { return ((ICollection<KeyValuePair<string, StoryVar>>)accessors).IsReadOnly; }
		}

		bool ICollection<KeyValuePair<string, StoryVar>>.Remove(KeyValuePair<string, StoryVar> item)
		{
			throw new NotSupportedException();
		}

		IEnumerator<KeyValuePair<string, StoryVar>> IEnumerable<KeyValuePair<string, StoryVar>>.GetEnumerator()
		{
			return accessors.Select(pair => new KeyValuePair<string, StoryVar>(pair.Key, pair.Value.Get())).GetEnumerator();
		}

		StoryVar IDictionary<string, StoryVar>.this[string key]
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

