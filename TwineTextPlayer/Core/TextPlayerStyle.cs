using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cradle.Players.Text
{
	[CreateAssetMenu]
	public class TextPlayerStyle : ScriptableObject
	{
		public string Text = "{0}";
		public string Link = "<link name=\"{0}\">{1}</link>";
		public string LineBreak = "\n";
		public string HtmlTag = "";
		public StyleFormatting[] Groups = new StyleFormatting[] {
			new StyleFormatting(new string[]{"italic", "em" }, "<i>", "</i>"),
			new StyleFormatting(new string[]{"bold", "strong" }, "<b>", "</b>"),
			new StyleFormatting("del", "<s>", "</s>"),
			new StyleFormatting("heading", "<b>", "</b>"),
			new StyleFormatting("sup", "<sup>", "</sup>"),
			new StyleFormatting("bulleted", "• <indent=15%>", "</indent>", "$1^"),
			new StyleFormatting("bulleted", "<indent=15%>◦ <indent=15%>", "</indent></indent>", "$2^"),
			new StyleFormatting("font", "<font=\"{0}\">", "</font>"),
			new StyleFormatting(new string[]{"color","colour", "textcolor", "textcolour" }, "<color=\"{0}\">", "</font>"),
		};
	}

	[System.Serializable]
	public class StyleFormatting
	{
		public string[] StyleNames;
		public string Prefix;
		public string Suffix;
		public string ValueRegex;

		public StyleFormatting() { }
		public StyleFormatting(string styleName, string prefix, string suffix, string regex = null):
			this(new string[] { styleName}, prefix, suffix, regex)
		{
		}

		public StyleFormatting(string[] styleNames, string prefix, string suffix, string regex = null)
		{
			StyleNames = styleNames;
			Prefix = prefix;
			Suffix = suffix;
		}
	}
}