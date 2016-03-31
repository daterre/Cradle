using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnityTwine.StoryFormats.Harlowe
{
	public class HarloweArray: ITwineVarType
	{
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

		public TwineVar this[string propertyName]
		{
			get
			{
				if (propertyName.ToLower() == "length")
					return this.Length;

				int index = HarloweUtils.PositionToIndex(propertyName, values.Count);
				try { return values[index]; }
				catch(System.IndexOutOfRangeException)
				{
					throw new System.IndexOutOfRangeException(string.Format("The array doesn't have a {0} position."));
				}
			}
			set
			{
				if (propertyName.ToLower() == "length")
					throw new TwineVarPropertyException("Cannot directly set the length of an array.");

				int index = HarloweUtils.PositionToIndex(propertyName, values.Count);
				try { values[index] = value; }
				catch (System.IndexOutOfRangeException)
				{
					throw new System.IndexOutOfRangeException(string.Format("The array doesn't have a {0} position."));
				}
			}
		}

		public bool Contains(TwineVar val)
		{
			throw new System.NotImplementedException();
		}
	}
}