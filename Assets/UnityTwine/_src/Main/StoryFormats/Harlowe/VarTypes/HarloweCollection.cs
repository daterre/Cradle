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
		public abstract IEnumerable<TwineVar> GetValues();

		protected bool TryGetMemberArray(TwineVar member, out TwineVar val)
		{
			// Special case when member is an array
			if (member.Value is HarloweArray)
			{
				var memberArray = (HarloweArray)member.Value;
				TwineVar[] valueArray = new TwineVar[memberArray.Length];
				for (int i = 0; i < memberArray.Length; i++)
					valueArray[i] = GetMember(memberArray.Values[i]);
				val = new HarloweArray(valueArray);
				return true;
			}
			else
			// Anything else treat as a property
			{
				val = default(TwineVar);
				return false;
			}
		}
	}
}