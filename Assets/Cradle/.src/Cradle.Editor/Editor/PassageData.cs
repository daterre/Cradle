using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Cradle.Editor
{
	[System.Serializable]
	public class PassageData
	{
		public string Pid;
		public string Name;
		public string Tags;
		public string Body;

		[System.NonSerialized]
		public PassageCode Code;
	}

	public class PassageCode
	{
		public string Main;
		public List<string> Fragments = new List<string>();
	}
}