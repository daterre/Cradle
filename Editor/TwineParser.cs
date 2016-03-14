using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityTwine.Editor
{
	public abstract class TwineParser
	{
		public readonly TwineImporter Importer;
		
		public TwineParser(TwineImporter importer)
		{
			this.Importer = importer;
		}

		public abstract string ParsePassageBody(string passageBody);
	}
}