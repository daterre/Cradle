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

		public static int PositionToIndex(string position, int total)
		{
			Match match = rx_Position.Match(position);
			if (!match.Success)
				throw new System.ArgumentException(string.Format("'{0}' is not a valid position", position));

			bool fromEnd = match.Groups["last"].Success;

			int index = 0;
			if (match.Groups["index"].Success)
				index = int.Parse(match.Groups["index"].Value) - 1;
			else if (!fromEnd)
				throw new System.ArgumentException(string.Format("'{0}' is not a valid position", position));

			if (fromEnd)
				index = total - 1 - index;

			return index;
		}
	}
}
