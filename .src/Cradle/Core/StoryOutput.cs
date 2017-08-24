using System;

namespace Cradle
{
	public abstract class StoryOutput
	{
		public string Name;
		public string Text;
		public int Index;

		public StyleGroup StyleGroup;
		public Embed EmbedInfo;

		
		public bool BelongsToStyleGroup(StyleGroup group)
		{
			if (group == null)
				throw new ArgumentNullException("group");

			StyleGroup parentGroup = this.StyleGroup;
			if (parentGroup == group)
				return true;
			else if (parentGroup != null)
				return parentGroup.BelongsToStyleGroup(group);
			else
				return false;
		}

		public Style GetAppliedStyle()
		{
			Style style = new Style();
			StyleGroup group = this.StyleGroup;
			while (group != null)
			{
				foreach (var entry in group.Style)
					if (!style.ContainsKey(entry.Key))
						style.Add(entry.Key, entry.Value);

				group = group.StyleGroup;
			}

			return style;
		}
	}
}

