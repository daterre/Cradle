using System;
using System.Text;

namespace UnityTwine
{
	public class TwineStyleTag: TwineOutput
	{
		public TwineStyleTagType TagType;
		public TwineStyle InnerStyle;

		string _desc;

		public TwineStyleTag(TwineStyleTagType tagType, TwineStyle innerStyle)
		{
			this.TagType = tagType;
			this.InnerStyle = innerStyle;

			// This is just for debugging purposes
			StringBuilder buffer = new StringBuilder(TagType == TwineStyleTagType.Opener ? "<style" : "</style");
			foreach (string setting in this.InnerStyle.SettingNames)
			{
				buffer.AppendFormat(" {0}='", setting);
				bool addedValue = false;
				foreach (object val in this.InnerStyle.GetValues(setting))
				{
					if (addedValue)
						buffer.Append(",");
					buffer.Append(val);
					addedValue = true;
				}
				buffer.Append("'");
			}
			buffer.Append(">");
			_desc = buffer.ToString();
		}

		public override string ToString()
		{
			return _desc;
		}
	}

	public enum TwineStyleTagType
	{
		Opener,
		Closer
	}
}

