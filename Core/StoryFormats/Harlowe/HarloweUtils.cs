using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UnityTwine.StoryFormats.Harlowe
{
	public static class HarloweUtils
	{
		static Regex rx_Position = new Regex(@"^(?'index'\d+)?(st|nd|rd|th)?(?'last'last)?$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

		public static bool TryPositionToIndex(string position, int total, out int index)
		{
			index = -1;

			Match match = rx_Position.Match(position);
			if (!match.Success)
				return false;

			bool fromEnd = match.Groups["last"].Success;
			
			if (match.Groups["index"].Success)
				index = int.Parse(match.Groups["index"].Value) - 1;
			else if (fromEnd)
				index = 0;
			else
				return false;

			if (fromEnd)
			{
				index = total - 1 - index;
			}

			return true;
		}

		public static int PositionToIndex(string position, int total)
		{
			int index = -1;
			if (!TryPositionToIndex(position, total, out index))
				throw new TwineTypePropertyException(string.Format("'{0}' is not a valid position", position));
			return index;
		}
	}
}
