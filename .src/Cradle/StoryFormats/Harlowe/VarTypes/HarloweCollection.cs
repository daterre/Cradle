using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace Cradle.StoryFormats.Harlowe
{
	public abstract class HarloweCollection : VarType
	{
		public abstract IEnumerable<StoryVar> GetValues();

		protected bool TryGetMemberArray(StoryVar member, out StoryVar val)
		{
			// Special case when member is an array
			if (member.Value is HarloweArray)
			{
				var memberArray = (HarloweArray)member.Value;
				StoryVar[] valueArray = new StoryVar[memberArray.Length];
				for (int i = 0; i < memberArray.Length; i++)
					valueArray[i] = GetMember(memberArray.Values[i]);
				val = new HarloweArray(valueArray);
				return true;
			}
			else
			// Anything else treat as a property
			{
				val = default(StoryVar);
				return false;
			}
		}
	}
}