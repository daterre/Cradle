using System;

namespace UnityTwine
{
	public class TwineStyleTag: TwineOutput
	{
		public TwineStyleTagType TagType;
		public TwineStyle InnerStyle;

		public TwineStyleTag(TwineStyleTagType tagType, TwineStyle innerStyle)
		{
			this.TagType = tagType;
			this.InnerStyle = innerStyle;
		}

		public override string ToString()
		{
			return TagType == TwineStyleTagType.Opener ? "<style>" : "</style>";
		}
	}

	public enum TwineStyleTagType
	{
		Opener,
		Closer
	}
}

