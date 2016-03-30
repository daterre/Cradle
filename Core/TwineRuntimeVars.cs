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

		protected Dictionary<string, TwineVar> dictionary = new Dictionary<string,TwineVar>();

		public TwineRuntimeVars(params string[] varNames)
		{
			for (int i = 0; i < varNames.Length; i++)
				dictionary[varNames[i]] = new TwineVar();
		}

		internal void Reset()
		{
			foreach(string varName in dictionary.Keys)
				dictionary[varName] = default(TwineVar);
		}

		public bool ContainsKey(string varName)
		{
			return dictionary.ContainsKey(varName);
		}

		public ICollection<string> Keys
		{
			get { return dictionary.Keys; }
		}

		public bool TryGetValue(string varName, out TwineVar value)
		{
			return dictionary.TryGetValue(varName, out value);
		}

		public ICollection<TwineVar> Values
		{
			get { return dictionary.Values; }
		}

		public TwineVar this[string varName]
		{
			get
			{
				var value = default(TwineVar);
				if (!TryGetValue(varName, out value))
					throw new TwineException(string.Format("There is no variable with the name '{0}'.", varName));
				return value;
			}
			set
			{
				TwineVar prevValue = this[varName];

				// Enfore strict mode
				if (StrictMode && value.value != null)
				{
					if (prevValue.value != null && !prevValue.value.GetType().IsAssignableFrom(value.value.GetType()))
						throw new TwineException(string.Format("Strict mode: the variable '{0}' was previously assigned a value of type {1}, and so cannot be assigned a value of type {2}.",
							varName,
							prevValue.value.GetType().Name,
							value.value.GetType().Name
						));
				}
				
				// Run the setter
				dictionary[varName] = value;
			}
		}

		public int Count
		{
			get { return dictionary.Count; }
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
			return ((ICollection<KeyValuePair<string, TwineVar>>)dictionary).Contains(item);
		}

		void ICollection<KeyValuePair<string, TwineVar>>.CopyTo(KeyValuePair<string, TwineVar>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<string, TwineVar>>)dictionary).CopyTo(array, arrayIndex);
		}

		bool ICollection<KeyValuePair<string, TwineVar>>.IsReadOnly
		{
			get { return ((ICollection<KeyValuePair<string, TwineVar>>)dictionary).IsReadOnly; }
		}

		bool ICollection<KeyValuePair<string, TwineVar>>.Remove(KeyValuePair<string, TwineVar> item)
		{
			throw new NotSupportedException();
		}

		IEnumerator<KeyValuePair<string, TwineVar>> IEnumerable<KeyValuePair<string, TwineVar>>.GetEnumerator()
		{
			return dictionary.GetEnumerator();
		}
	}
}

