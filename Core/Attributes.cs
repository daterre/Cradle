using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityTwine
{
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
	public sealed class TwineMacroLibraryAttribute : Attribute
	{
		public readonly Type StoryType;

		public TwineMacroLibraryAttribute(Type storyType)
		{
			if (!typeof(TwineStory).IsAssignableFrom(storyType))
				throw new ArgumentException("TwineMacroLibary attribute requires a type deriving from TwineStory.", "storyType");
			
			this.StoryType = storyType;
		}
	}

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
