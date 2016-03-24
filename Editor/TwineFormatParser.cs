using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityTwine.Editor
{
	public abstract class TwineFormatParser
	{
		public readonly TwineImporter Importer;
		
		public TwineFormatParser(TwineImporter importer)
		{
			this.Importer = importer;
		}

		public virtual void Init() { }
		public abstract TwinePassageCode PassageToCode(TwinePassageData passage);
	}
}