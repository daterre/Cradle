using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cradle.Players
{
	[CreateAssetMenu(fileName = "New Twine TMPro Style Sheet", menuName = "Cradle/Twine TMPro Style Sheet", order = 1000)]
	public class TwineTMProStyleSheet : ScriptableObject
	{
		public static string DefaultLink = "<link=\"{0}\">{1}</link>";
		public static string DefaultLineBreak = @"\n";

		public string Text = "{0}";
		public string Link = DefaultLink;
		public string LineBreak = DefaultLineBreak;
		public string HtmlTag = "";

		public TwineTMProStyle[] Styles;
	}
}