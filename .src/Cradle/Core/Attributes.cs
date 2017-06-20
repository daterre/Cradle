using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cradle
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public sealed class StoryCueAttribute : Attribute
	{
		public string PassageName;
		public string LinkName;
		public CueType Cue;

		public StoryCueAttribute(string passageName, string cueName) :
			this(passageName, null, cueName)
		{
		}

		public StoryCueAttribute(string passageName, CueType cue) :
			this(passageName, null, cue)
		{
		}

		public StoryCueAttribute(string passageName, string linkName, string cueName):
			this(passageName, linkName, (CueType)Enum.Parse(typeof(CueType), cueName))
		{
		}

		public StoryCueAttribute(string passageName, string linkName, CueType cue)
		{
			this.PassageName = passageName;
			this.LinkName = linkName;
			this.Cue = cue;
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
