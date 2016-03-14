using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace UnityTwine.Editor
{
	public abstract class TwineImporter
	{
		public class PassageData
		{
			public int ID;
			public string Name;
			public string Tags;
			public string Body;
			public string[] Code;
		}

		public readonly string AssetPath;
		public TwineParser Parser {get; protected set;}
		
		public readonly List<PassageData> Passages = new List<PassageData>();
		public readonly Dictionary<string, string> Vars = new Dictionary<string, string>();

		public TwineImporter(string assetPath)
		{
			this.AssetPath = assetPath;
		}

		public abstract void Load();
		public abstract void Prepare();
		
		public void Parse()
		{
			if (this.Parser == null)
				throw new UnityException("TwineImporter.Parser must be set by the importer implementation.");

			for (int i = 0; i < this.Passages.Count; i++)
			{
				PassageData passage = this.Passages[i];
				passage.Tags = Regex.Replace(passage.Tags, @"([^\s]+)", "\"$&\",");
				passage.Code = this.Parser.ParsePassageBody(passage.Body).Split('\n');
			}
		}
	}
}