using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cradle.Players.TextMeshPro
{
	[CreateAssetMenu(fileName = "New TMPro Format Settings", menuName = "Cradle/TMPro Format Settings", order = 1000)]
	public class TMProFormatSettings : ScriptableObject
	{
		public string Text = "{0}";
		public string Link = "<link=\"{0}\">{1}</link>";
		public string LineBreak = @"\n";
		public string HtmlTag = "";
		public StyleFormat[] Styles = new StyleFormat[] {
			new StyleFormat(new string[]{"italic", "em" }, "<i>", "</i>"),
			new StyleFormat(new string[]{"bold", "strong" }, "<b>", "</b>"),
			new StyleFormat("del", "<s>", "</s>"),
			new StyleFormat("heading", "<b>", "</b>"),
			new StyleFormat("sup", "<sup>", "</sup>"),
			new StyleFormat("bulleted", "• <indent=15%>", "</indent>", "$1^"),
			new StyleFormat("bulleted", "<indent=15%>◦ <indent=15%>", "</indent></indent>", "$2^"),
			new StyleFormat("font", "<font=\"{0}\">", "</font>"),
			new StyleFormat(new string[]{"color","colour", "textcolor", "textcolour" }, "<color=\"{0}\">", "</color>"),
		};
	}

	[System.Serializable]
	public class StyleFormat
	{
		public string[] MatchingKeys;
		public string MatchingValuesRegex;
		public string Prefix;
		public string Suffix;

		public StyleFormat() { }
		public StyleFormat(string key, string prefix, string suffix, string regex = null):
			this(new string[] { key}, prefix, suffix, regex)
		{
		}

		public StyleFormat(string[] keys, string prefix, string suffix, string regex = null)
		{
			MatchingKeys = keys;
			Prefix = prefix;
			Suffix = suffix;
		}
	}
}