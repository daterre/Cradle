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
		static Regex rx_Passages = new Regex(@"^::\s(?<name>[^\]\|\n]+)(\s+\[(?<tags>[^\]]+)\])?\n(?<body>.*?)(?=\n::|\Z)",
			RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.ExplicitCapture);

		public TweeImporter(string assetPath): base(assetPath)
		{
		}

		public override void Load()
		{
			string tweeSource = File.ReadAllText(this.AssetPath);

			MatchCollection matches = rx_Passages.Matches(tweeSource);

			for (int i = 0; i < matches.Count; i++)
			{
				Match m = matches[i];

				// TODO: move this to SugarCube-related code
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

			// TODO: sniff story format
			this.Parser = new SugarCubeParser(this);
		}
	}
}