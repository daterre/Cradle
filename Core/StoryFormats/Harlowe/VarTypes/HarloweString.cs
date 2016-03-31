using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityTwine;

namespace UnityTwine.StoryFormats.Harlowe
{
	public class HarloweStringService: TwineVarTypeService
	{
		public override TwineVar GetProperty(object container, string propertyName)
		{
			string containerString = (string)container;

			switch (propertyName.ToLower())
			{
				case "length":
					return containerString.Length;
				default:
					return containerString[HarloweUtils.PositionToIndex(propertyName, containerString.Length)].ToString();
			}
		}

		public override void SetProperty(object container, string propertyName, TwineVar value)
		{
			throw new TwineVarPropertyException("Cannot set any properties of a string.");
		}

		public override bool Contains(object container, object containee)
		{
			string containerString = (string)container;
			return containerString.Contains((string)containee);
		}

	}
}
