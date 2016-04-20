using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace UnityTwine.StoryFormats.Harlowe
{
	public abstract class HarloweCollection : TwineType
	{
		public abstract IEnumerable<TwineVar> Flatten();
	}
}