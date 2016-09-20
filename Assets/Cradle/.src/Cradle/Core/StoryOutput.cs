using System;

namespace Cradle
{
	public abstract class StoryOutput
	{
		public string Name;
		public string Text;
		public int Index;

		public OutputGroup Group;
		public Embed EmbedInfo;

		public bool BelongsToGroup(OutputGroup group)
		{
			if (group == null)
				throw new ArgumentNullException("group");

			OutputGroup parentGroup = this.Group;
			if (parentGroup == group)
				return true;
			else if (parentGroup != null)
				return parentGroup.BelongsToGroup(group);
			else
				return false;
		}

		public StoryStyle GetAppliedStyle()
		{
			StoryStyle style = new StoryStyle();
			OutputGroup group = this.Group;
			while (group != null)
			{
				foreach (var entry in group.Style)
					if (!style.ContainsKey(entry.Key))
						style.Add(entry.Key, entry.Value);

				group = group.Group;
			}

			return style;
		}
	}
}

