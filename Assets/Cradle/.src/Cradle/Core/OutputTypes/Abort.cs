using System;

namespace Cradle
{
	public class Abort: StoryOutput
	{
		public string GoToPassage = null;

		public Abort(string goToPassage)
		{
			this.GoToPassage = goToPassage;
		}
	}
}

