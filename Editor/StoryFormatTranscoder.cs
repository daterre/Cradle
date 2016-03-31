using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityTwine.Editor
{
	public abstract class StoryFormatTranscoder
	{
		public readonly TwineImporter Importer;
		
		public StoryFormatTranscoder(TwineImporter importer)
		{
			this.Importer = importer;
		}

		public abstract StoryFormatMetadata Metadata { get; }
		public virtual void Init() { }
		public abstract TwinePassageCode PassageToCode(TwinePassageData passage);
	}

	public class StoryFormatMetadata
	{
		public string StoryFormatName;
		public Type StoryBaseType = typeof(TwineStory);
		public Type RuntimeMacrosType = typeof(TwineRuntimeMacros);
		public bool StrictMode = false;
	}
}