using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Cradle
{
	public static class Extensions
	{
		// .................................
		// Enums

		public static bool In(this Enum value, params Enum[] values)
		{
			for (int i = 0; i < values.Length; i++)
				if (object.Equals(values[i], value))
					return true;
			return false;
		}

		public static bool HasFlag(this Enum val, Enum test)
		{
			try { return (Convert.ToInt32(val) & Convert.ToInt32(test)) > 0; }
			catch { Debug.LogError(val); throw; }
		}

		public static IEnumerable<Enum> GetFlags(this Enum value)
		{
			return GetFlags(value, Enum.GetValues(value.GetType()).Cast<Enum>().ToArray());
		}

		public static IEnumerable<Enum> GetIndividualFlags(this Enum value)
		{
			return GetFlags(value, GetFlagValues(value.GetType()).ToArray());
		}

		private static IEnumerable<Enum> GetFlags(Enum value, Enum[] values)
		{
			ulong bits = Convert.ToUInt64(value);
			List<Enum> results = new List<Enum>();
			for (int i = values.Length - 1; i >= 0; i--)
			{
				ulong mask = Convert.ToUInt64(values[i]);
				if (i == 0 && mask == 0L)
					break;
				if ((bits & mask) == mask)
				{
					results.Add(values[i]);
					bits -= mask;
				}
			}
			if (bits != 0L)
				return Enumerable.Empty<Enum>();
			if (Convert.ToUInt64(value) != 0L)
				return results.Reverse<Enum>();
			if (bits == Convert.ToUInt64(value) && values.Length > 0 && Convert.ToUInt64(values[0]) == 0L)
				return values.Take(1);
			return Enumerable.Empty<Enum>();
		}

		private static IEnumerable<Enum> GetFlagValues(Type enumType)
		{
			ulong flag = 0x1;
			foreach (var value in Enum.GetValues(enumType).Cast<Enum>())
			{
				ulong bits = Convert.ToUInt64(value);
				if (bits == 0L)
					//yield return value;
					continue; // skip the zero value
				while (flag < bits) flag <<= 1;
				if (flag == bits)
					yield return value;
			}
		}
	}
}
