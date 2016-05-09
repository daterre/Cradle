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
			return string.Format("{0} (text)", this.Text);
		}
	}
}

