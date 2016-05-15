using System;

namespace UnityTwine
{
	public class TwineText: TwineOutput
	{
		public TwineText(string text)
		{
			this.Text = text;
		}

		public override string ToString()
		{
			if (this.Text != null && this.Text.Trim().Length < 1)
				return "(whitespace)";
			else
				return string.Format("{0} (text)", this.Text ?? "(null)");
		}
	}
}

