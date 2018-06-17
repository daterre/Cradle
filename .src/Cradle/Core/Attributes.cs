using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cradle
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public class CueAttribute : Attribute
	{
		public CueType CueType { get; protected set; }
		public string Passage { get; set; }
		public string Link { get; set; }
		public string Tag { get; set; }
		public bool Regex { get; set; }
		public int Order { get; set; }

		public CueAttribute(CueType cueType)
		{
			this.CueType = cueType;
			this.Order = 0;
			this.Regex = false;
		}
	}

	[Obsolete("Use [Cue] attribute instead")]
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public sealed class StoryCueAttribute : CueAttribute
	{
		public StoryCueAttribute(string passageName, string cueName) :
			this(passageName, null, cueName)
		{
		}

		public StoryCueAttribute(string passageName, CueType cueType) :
			this(passageName, null, cueType)
		{
		}

		public StoryCueAttribute(string passageName, string linkName, string cueName) :
			this(passageName, linkName, (CueType)Enum.Parse(typeof(CueType), cueName))
		{
		}

		public StoryCueAttribute(string passageName, string linkName, CueType cueType):
			base(cueType)
		{
			this.Passage = passageName;
			this.Link = linkName;

			switch (cueType)
			{
				case CueType.Enter:
					this.CueType = string.IsNullOrEmpty(linkName) ? CueType.PassageEnter : CueType.LinkBegin;
					break;
				case CueType.Done:
					this.CueType = string.IsNullOrEmpty(linkName) ? CueType.PassageDone : CueType.LinkDone;
					break;
			}
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
