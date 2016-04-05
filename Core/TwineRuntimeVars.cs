using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace UnityTwine
{
	public class TwineRuntimeVars: ITwineType, IDictionary<string, TwineVar>
	{
		public TwineStory Story;
		public bool StrictMode;

		protected Dictionary<string, TwineVar> dictionary = new Dictionary<string,TwineVar>();

		public TwineRuntimeVars(params string[] varNames)
		{
			for (int i = 0; i < varNames.Length; i++)
				dictionary[varNames[i]] = new TwineVar(this, varNames[i]);
		}

		internal void Reset()
		{
			foreach(string varName in dictionary.Keys)
				dictionary[varName] = new TwineVar(this, varName);
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
			object v = value.Value;

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
			dictionary[varName] = new TwineVar(this, varName, v);
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

		void ITwineType.RemoveMember(string varName)
		{
			if (!dictionary.ContainsKey(varName))
				throw new TwineException(string.Format("There is no variable with the name '{0}'.", varName));

			dictionary[varName] = new TwineVar(this, varName);
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

