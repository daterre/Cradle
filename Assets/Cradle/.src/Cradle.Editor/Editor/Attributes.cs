using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cradle.Editor
{
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public sealed class CodeGenMacroAttribute : Attribute
	{
		public readonly string TwineName;

		public CodeGenMacroAttribute(string twineName)
		{
			this.TwineName = twineName;
		}

		public CodeGenMacroAttribute()
		{
		}
	}
}
