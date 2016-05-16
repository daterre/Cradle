using System;
using System.Text;

namespace Cradle
{
	public class StyleTag: StoryOutput
	{
		public StyleTagType TagType;
		public StoryStyle InnerStyle;

		string _desc;

		public StyleTag(StyleTagType tagType, StoryStyle innerStyle)
		{
			this.TagType = tagType;
			this.InnerStyle = innerStyle;

			// This is just for debugging purposes
			StringBuilder buffer = new StringBuilder(TagType == StyleTagType.Opener ? "<style" : "</style");
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

	public enum StyleTagType
	{
		Opener,
		Closer
	}
}

