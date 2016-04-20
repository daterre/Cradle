using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using UnityTwine.Editor.StoryFormats;

namespace UnityTwine.Editor.Importers
{
	public class TweeImporter : TwineImporter
	{
		static TweeImporter()
		{
			TwineAssetProcessor.RegisterImporter<TweeImporter>("twee");
		}

		static Regex rx_Passages = new Regex(@"^::\s(?<name>[^\]\|\n]+)(\s+\[(?<tags>[^\]]+)\])?\n(?<body>.*?)(?=\n::|\Z)",
			RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.ExplicitCapture);

		public override void Initialize()
		{
			string tweeSource = File.ReadAllText(this.AssetPath);

			MatchCollection matches = rx_Passages.Matches(tweeSource);
			if (matches.Count < 1)
				throw new TwineImportException("Twee data could not be found.");

			for (int i = 0; i < matches.Count; i++)
			{
				Match m = matches[i];

				// Ignore images
				if (m.Groups["tags"].Success && m.Groups["tags"].Value == "Twine.image")
					continue;

				this.Passages.Add(new TwinePassageData()
				{
					Pid = i.ToString(),
					Name = m.Groups["name"].Value,
					Tags = m.Groups["tags"].Value,
					Body = m.Groups["body"].Value.Trim()
				});
			}

			// Twee only uses the Sugar transcoder
			this.Transcoder = new StoryFormats.Sugar.SugarTranscoder() { Importer = this };
		}
	}
}