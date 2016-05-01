using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityTwine;

namespace UnityTwine.StoryFormats.Harlowe
{
	public class HarloweStringService: StringService
	{
		public override TwineVar GetMember(string container, TwineVar member)
		{
			// Special case when member is an array
			if (member.Value is HarloweArray)
			{
				var memberArray = (HarloweArray)member.Value;
				var buffer = new char[memberArray.Length];
				for (int i = 0; i < memberArray.Length; i++)
					buffer[i] = GetMember(container, TwineVar.ConvertTo<int>(memberArray.Values[i])).ToString()[0];

				return new string(buffer);
			}

			int index;
			if (HarloweUtils.TryPositionToIndex(member, container.Length, out index))
				return new TwineVar(container[index].ToString());

			return base.GetMember(container, member);
		}
	}
}
