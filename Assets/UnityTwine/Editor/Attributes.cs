using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityTwine.Editor
{
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public sealed class TwineCodeGenMacroAttribute : Attribute
	{
		public readonly string TwineName;

		public TwineCodeGenMacroAttribute(string twineName)
		{
			this.TwineName = twineName;
		}

		public TwineCodeGenMacroAttribute()
		{
		}
	}
}
