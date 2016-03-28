using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnityTwine.StoryFormats.Harlowe
{
	public class HarloweArray
	{
		static Regex rx_Position = new Regex(@"^(?'index'[\d]*)(st|nd|rd|th)?(?'last'last)?$", RegexOptions.IgnoreCase);

		List<TwineVar> values;
		

		public HarloweArray()
		{
			values = new List<TwineVar>();
		}

		public HarloweArray(params TwineVar[] vals)
		{
			values = new List<TwineVar>(vals);
		}

		public int Length
		{
			get { return values.Count; }
		}

		public TwineVar this[string position]
		{
			get
			{
				int index = PositionToIndex(position);
				try { return values[index]; }
				catch(System.IndexOutOfRangeException)
				{
					throw new System.IndexOutOfRangeException(string.Format("The array doesn't have a {0} position."));
				}
			}
			set
			{
				int index = PositionToIndex(position);
				try { values[index] = value; }
				catch (System.IndexOutOfRangeException)
				{
					throw new System.IndexOutOfRangeException(string.Format("The array doesn't have a {0} position."));
				}
			}
		}

		int PositionToIndex(string position)
		{
			Match match = rx_Position.Match(position);
			if (!match.Success)
				throw new System.ArgumentException(string.Format("'{0}' is not a valid array position", position));

			bool fromEnd = match.Groups["last"].Success;

			int index = 0;
			if (match.Groups["index"].Success)
				index = int.Parse(match.Groups["index"].Value)-1;
			else if (!fromEnd)
				throw new System.ArgumentException(string.Format("'{0}' is not a valid array position", position));

			if (fromEnd)
				index = values.Count - 1 - index;

			return index;
		}
	}
}