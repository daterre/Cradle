using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cradle
{
	#region Obsolete
	[Obsolete("Use PassageCue, LinkCue or TagCue")]
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public sealed class StoryCueAttribute : PassageCueAttribute
	{
		public StoryCueAttribute(string passageName, string cueName, int order = 0) :
			this(passageName, null, cueName, order)
		{
		}

		public StoryCueAttribute(string passageName, CueType cue, int order = 0) :
			this(passageName, null, cue, order)
		{
		}

		public StoryCueAttribute(string passageName, string linkName, string cueName, int order = 0) :
			this(passageName, linkName, (CueType)Enum.Parse(typeof(CueType), cueName), order)
		{
		}

		public StoryCueAttribute(string passageName, string linkName, CueType cue, int order = 0):
			base(passageName, linkName, cue, order)
		{
		}
	}
	#endregion

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public class LinkCueAttribute : Attribute
	{
		public string LinkName;
		public CueType Cue;
		public int Order = 0;

		public LinkCueAttribute(string linkName, CueType cue, int order = 0)
		{
			this.LinkName = linkName;
			this.Cue = cue;
			this.Order = order;
		}
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public class PassageCueAttribute : LinkCueAttribute
	{
		public string PassageName;

		public PassageCueAttribute(string passageName, CueType cue, int order = 0) :
			this(passageName, null, cue, order)
		{
		}

		public PassageCueAttribute(string passageName, string linkName, CueType cue, int order = 0):
			base(linkName, cue, order)
		{
			this.PassageName = passageName;
		}
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public class TagCueAttribute : LinkCueAttribute
	{
		public string TagName;

		public TagCueAttribute(string tagName, CueType cue, int order = 0) :
			this(tagName, null, cue, order)
		{
		}

		public TagCueAttribute(string tagName, string linkName, CueType cue, int order = 0):
			base(linkName, cue, order)
		{
			this.TagName = tagName;
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
