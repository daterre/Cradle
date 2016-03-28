using UnityEngine;
using System.Collections;

namespace UnityTwine.StoryFormats.Harlowe
{
	public class HarloweRuntimeMacros: TwineRuntimeMacros
	{
		public void @goto(string passageName)
		{
			throw new System.NotImplementedException();
		}

		public void @click(string passageName)
		{
			throw new System.NotImplementedException();
		}

		public TwineVar @ceil(double num)
		{
			return Mathf.Ceil((float)num);
		}

		public TwineVar a(params TwineVar[] vals)
		{
			// Wrap a Harlowe array in a TwineVar
			return new TwineVar(new HarloweArray(vals));
		}
	}
}

