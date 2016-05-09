using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityTwine
{
	public static class Extensions
	{
		public static bool In(this Enum value, params Enum[] values)
		{
			for (int i = 0; i < values.Length; i++)
				if (object.Equals(values[i], value))
					return true;
			return false;
		}
	}
}
