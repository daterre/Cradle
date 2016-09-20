using System;
using System.Text;

namespace Cradle
{
	public class OutputGroup: StoryOutput
	{
		public StoryStyle Style;

		public OutputGroup(StoryStyle style)
		{
			this.Style = style;
		}

		public override string ToString()
		{
			// This is just for debugging purposes
			StringBuilder buffer = new StringBuilder("(group: ");
			foreach (var entry in this.Style)
			{
				buffer.AppendFormat(" {0}='{1}'", entry.Key, entry.Value);
			}
			buffer.Append(")");
			return buffer.ToString();
		}
	}
}