using System;

namespace Cradle
{
	public class HtmlTag: StoryOutput
	{
		public HtmlTag(string text)
		{
			this.Text = text;
		}

		public override string ToString()
		{
			if (this.Text != null && this.Text.Trim().Length < 1)
				return "(whitespace)";
			else
				return string.Format("{0} (tag)", this.Text ?? "(null)");
		}
	}
}

