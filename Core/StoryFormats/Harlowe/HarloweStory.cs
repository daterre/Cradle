using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITwineThread = System.Collections.Generic.IEnumerable<UnityTwine.TwineOutput>;

namespace UnityTwine.StoryFormats.Harlowe
{
	public class HarloweStory: TwineStory
	{
		public HarloweStory()
		{
			TwineVar.RegisterTypeService<string>(new HarloweStringService());
		}

		protected TwineVar hookRef(TwineVar hookName)
		{
			return new HarloweHookRef(hookName);
		}
	}
}
