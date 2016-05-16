using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cradle
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class StoryCueAttribute : Attribute
	{
		public string CueName;

		public StoryCueAttribute(string cueName)
		{
			this.CueName = cueName;
		}
	}


	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
	public sealed class MacroLibraryAttribute : Attribute
	{
		public readonly Type StoryType;

		public MacroLibraryAttribute(Type storyType)
		{
			if (!typeof(Story).IsAssignableFrom(storyType))
				throw new ArgumentException("MacroLibary attribute requires a type deriving from Story.", "storyType");
			
			this.StoryType = storyType;
		}
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	public sealed class RuntimeMacroAttribute : Attribute
	{
		public readonly string TwineName;

		public RuntimeMacroAttribute(string twineName)
		{
			this.TwineName = twineName;
		}

		public RuntimeMacroAttribute()
		{
		}
	}
}
