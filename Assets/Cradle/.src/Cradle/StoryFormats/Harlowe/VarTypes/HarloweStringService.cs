using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cradle;

namespace Cradle.StoryFormats.Harlowe
{
	public class HarloweStringService: StringService
	{
		public override StoryVar GetMember(string container, StoryVar member)
		{
			// Special case when member is an array
			if (member.Value is HarloweArray)
			{
				var memberArray = (HarloweArray)member.Value;
				var buffer = new char[memberArray.Length];
				for (int i = 0; i < memberArray.Length; i++)
					buffer[i] = GetMember(container, StoryVar.ConvertTo<int>(memberArray.Values[i])).ToString()[0];

				return new string(buffer);
			}

			int index;
			if (HarloweUtils.TryPositionToIndex(member, container.Length, out index))
				return new StoryVar(container[index].ToString());

			return base.GetMember(container, member);
		}
	}
}
