using System;

namespace UnityTwine
{
	public class TwineAbort: TwineOutput
	{
		public string GoToPassage = null;

		public TwineAbort(string goToPassage)
		{
			this.GoToPassage = goToPassage;
		}
	}
}

