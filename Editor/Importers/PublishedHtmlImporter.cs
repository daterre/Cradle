using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using UnityTwine.Editor.StoryFormats.Harlowe;

namespace UnityTwine.Editor.Importers
{
	public class PublishedHtmlImporter : TwineImporter
	{
		public PublishedHtmlImporter(string assetPath) : base(assetPath)
		{
		}

		public override void Load()
		{
			// TODO: sniff story format
			this.Parser = new HarloweParser(this);
		}
	}
}