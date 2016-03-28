using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityTwine.Editor
{
	public abstract class TwineFormatTranscoder
	{
		public readonly TwineImporter Importer;
		
		public TwineFormatTranscoder(TwineImporter importer)
		{
			this.Importer = importer;
		}

		public virtual string RuntimeMacrosClassName { get { return "TwineRuntimeMacros"; } }
		public virtual void Init() { }
		public abstract TwinePassageCode PassageToCode(TwinePassageData passage);
	}
}