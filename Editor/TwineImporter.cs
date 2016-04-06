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
		public TwinePassageData CurrentPassage { get; private set; }

		public readonly List<TwinePassageData> Passages = new List<TwinePassageData>();
		public readonly HashSet<string> Vars = new HashSet<string>();

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
				CurrentPassage = this.Passages[i];

				CurrentPassage.Tags = Regex.Replace(CurrentPassage.Tags, @"([^\s]+)", "\"$&\",");

				try
				{
					CurrentPassage.Code = this.Transcoder.PassageToCode(CurrentPassage);
				}
				catch(TwineTranscodingException ex)
				{
					ex.Passage = CurrentPassage.Name;
					throw;
				}
			}
		}

		public void RegisterVar(string name)
		{
			Vars.Add(name);
		}

	}
}