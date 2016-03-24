using UnityEngine;
using System.Collections;
using System.Text;

namespace UnityTwine.Editor.Utils
{
	public static class CodeGen
	{
		public static string Indent(int num, StringBuilder buffer = null)
		{
			var tabsTemp = buffer ?? new StringBuilder();
			for (int i = 0; i < Mathf.Max(0, num); i++)
				tabsTemp.Append('\t');
			string tabs = tabsTemp.ToString();
			return tabs;
		}
	}
}
