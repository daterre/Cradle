using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace UnityTwine
{
	public abstract class TwineRuntimeVars: ITwineType, IDictionary<string, TwineVarRef>
	{
		public TwineStory Story;
		public bool StrictMode;

		protected Dictionary<string, TwineVarRef> dictionary = new Dictionary<string,TwineVarRef>();

		public TwineRuntimeVars(params string[] varNames)
		{
			for (int i = 0; i < varNames.Length; i++)
				dictionary[varNames[i]] = new TwineVarRef(this, varNames[i]);
		}

		internal void Reset()
		{
			foreach(string varName in dictionary.Keys)
				dictionary[varName] = new TwineVarRef(this, varName);
		}

		public bool ContainsKey(string varName)
		{
			return dictionary.ContainsKey(varName);
		}

		public ICollection<string> Keys
		{
			get { return dictionary.Keys; }
		}

		public bool TryGetValue(string varName, out TwineVarRef value)
		{
			return dictionary.TryGetValue(varName, out value);
		}

		public ICollection<TwineVarRef> Values
		{
			get { return dictionary.Values; }
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

		public TwineVarRef GetMember(string varName)
		{
			var value = default(TwineVarRef);
			if (!TryGetValue(varName, out value))
				throw new TwineException(string.Format("There is no variable with the name '{0}'.", varName));
			return value;
		}

		public void SetMember(string varName, TwineVar value)
		{
			TwineVar prevValue = this[varName];

			// Enfore strict mode
			if (StrictMode && value.Value != null)
			{
				if (prevValue.Value != null && !prevValue.Value.GetType().IsAssignableFrom(value.Value.GetType()))
					throw new TwineStrictModeException(string.Format("The variable '{0}' was previously assigned a value of type {1}, and so cannot be assigned a value of type {2}.",
						varName,
						prevValue.Value.GetType().Name,
						value.Value.GetType().Name
					));
			}

			// Run the setter
			dictionary[varName] = new TwineVarRef(this, varName, value);
		}

		public int Count
		{
			get { return dictionary.Count; }
		}

		void IDictionary<string, TwineVarRef>.Add(string key, TwineVarRef value)
		{
			throw new NotSupportedException();
		}

		bool IDictionary<string, TwineVarRef>.Remove(string key)
		{
			throw new NotSupportedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		void ICollection<KeyValuePair<string, TwineVarRef>>.Add(KeyValuePair<string, TwineVarRef> item)
		{
			throw new NotSupportedException();
		}

		void ICollection<KeyValuePair<string, TwineVarRef>>.Clear()
		{
			throw new NotSupportedException();
		}

		bool ICollection<KeyValuePair<string, TwineVarRef>>.Contains(KeyValuePair<string, TwineVarRef> item)
		{
			return ((ICollection<KeyValuePair<string, TwineVarRef>>)dictionary).Contains(item);
		}

		void ICollection<KeyValuePair<string, TwineVarRef>>.CopyTo(KeyValuePair<string, TwineVarRef>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<string, TwineVarRef>>)dictionary).CopyTo(array, arrayIndex);
		}

		bool ICollection<KeyValuePair<string, TwineVarRef>>.IsReadOnly
		{
			get { return ((ICollection<KeyValuePair<string, TwineVarRef>>)dictionary).IsReadOnly; }
		}

		bool ICollection<KeyValuePair<string, TwineVarRef>>.Remove(KeyValuePair<string, TwineVarRef> item)
		{
			throw new NotSupportedException();
		}

		IEnumerator<KeyValuePair<string, TwineVarRef>> IEnumerable<KeyValuePair<string, TwineVarRef>>.GetEnumerator()
		{
			return dictionary.GetEnumerator();
		}

		void ITwineType.RemoveMember(string memberName)
		{
			this[memberName] = new TwineVarRef(this, memberName);
		}

		bool ITwineType.Compare(TwineOperator op, object b, out bool result)
		{
			throw new NotSupportedException();
		}

		bool ITwineType.Combine(TwineOperator op, object b, out TwineVar result)
		{
			throw new NotSupportedException();
		}

		bool ITwineType.Unary(TwineOperator op, out TwineVar result)
		{
			throw new NotSupportedException();
		}

		bool ITwineType.ConvertTo(Type t, out object result, bool strict)
		{
			throw new NotSupportedException();
		}

		TwineVarRef IDictionary<string, TwineVarRef>.this[string key]
		{
			get
			{
				return GetMember(key);
			}
			set
			{
				SetMember(key, value.Value);
			}
		}
	}
}

