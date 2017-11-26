using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cradle.Players
{
	[CreateAssetMenu(fileName = "New Twine TMPro Style", menuName = "Cradle/Twine TMPro Style", order = 1000)]
	public class TwineTMProStyle: ScriptableObject
	{
		public string[] MatchingKeys;
		public string MatchingValuesRegex;
		public string Prefix;
		public string Suffix;
	}
}