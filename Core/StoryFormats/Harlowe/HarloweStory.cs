using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityTwine.StoryFormats.Harlowe
{
	public class HarloweStory: TwineStory
	{
		public HarloweStory()
		{
			TwineVar.RegisterTypeService<string>(new HarloweStringService());
		}
	}
}
