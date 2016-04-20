using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityTwine;

namespace UnityTwine.StoryFormats.Harlowe
{
	public class HarloweStringService: StringService
	{
		public override TwineVar GetMember(string container, string memberName)
		{
			int index;
			if (HarloweUtils.TryPositionToIndex(memberName, container.Length, out index))
				return new TwineVar(container[index].ToString());

			return base.GetMember(container, memberName);
		}
	}
}
