using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityTwine.Editor
{
	[System.Serializable]
	public class TwinePassageData
	{
		public string Pid;
		public string Name;
		public string Tags;
		public string Body;

		[System.NonSerialized]
		public TwinePassageCode Code;
	}

	public class TwinePassageCode
	{
		public string Main;
		public List<string> Fragments = new List<string>();
	}
}