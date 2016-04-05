using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace UnityTwine.Editor
{
	public abstract class TwineImporter
	{
		public readonly string AssetPath;
		public StoryFormatTranscoder Transcoder {get; protected set;}

		public readonly List<TwinePassageData> Passages = new List<TwinePassageData>();
		public readonly Dictionary<string, string> Vars = new Dictionary<string, string>();

		public TwineImporter(string assetPath)
		{
			this.AssetPath = assetPath;
		}

		public virtual bool Validate() { return true; }
		public abstract void Load();
		
		public void Transcode()
		{
			if (this.Transcoder == null)
				throw new System.NotImplementedException("TwineImporter.Transcoder must be set by the importer implementation.");

			this.Transcoder.Init();

			for (int i = 0; i < this.Passages.Count; i++)
			{
				TwinePassageData passage = this.Passages[i];

				passage.Tags = Regex.Replace(passage.Tags, @"([^\s]+)", "\"$&\",");

				try
				{
					passage.Code = this.Transcoder.PassageToCode(passage);
				}
				catch(TwineTranscodingException ex)
				{
					ex.Passage = passage.Name;
					throw;
				}
			}
		}

		public void RegisterVar(string name)
		{
			Vars[name] = null; // null because we don't need any value here, just using a dictionary as a lookup
		}

	}
}