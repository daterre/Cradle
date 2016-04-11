using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityTwine
{
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	public sealed class TwineRuntimeMacroAttribute : Attribute
	{
		public readonly string TwineName;

		public TwineRuntimeMacroAttribute(string twineName)
		{
			this.TwineName = twineName;
		}

		public TwineRuntimeMacroAttribute()
		{
		}
	}
}
