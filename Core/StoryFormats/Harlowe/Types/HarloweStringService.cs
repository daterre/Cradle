using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityTwine;

namespace UnityTwine.StoryFormats.Harlowe
{
	public class HarloweStringService: StringService
	{
		public override TwineVar GetProperty(string container, string propertyName)
		{
			int index;
			if (HarloweUtils.TryPositionToIndex(propertyName, container.Length, out index))
				return container[index].ToString();

			return base.GetProperty(container, propertyName);
		}
	}
}
