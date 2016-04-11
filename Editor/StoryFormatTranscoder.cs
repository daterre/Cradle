using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnityTwine.Editor
{
	public abstract class StoryFormatTranscoder
	{
		public readonly TwineImporter Importer;
		
		public StoryFormatTranscoder(TwineImporter importer)
		{
			this.Importer = importer;
		}

		public abstract StoryFormatMetadata Metadata { get; }
		public virtual void Init() { }
		public abstract TwinePassageCode PassageToCode(TwinePassageData passage);

		static Regex _cSharpReservedWords = new Regex(@"^(abstract|as|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|finally|fixed|float|for|foreach|goto|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|virtual|void|volatile|while|add|alias|ascending|async|await|descending|dynamic|from|get|global|group|into|join|let|orderby|partial|remove|select|set|value|var|where|yield)$");
		public static string EscapeReservedWord(string name)
		{
			if (_cSharpReservedWords.IsMatch(name))
				name = "@" + name;
			return name;
		}
	}

	public class StoryFormatMetadata
	{
		public string StoryFormatName;
		public Type StoryBaseType = typeof(TwineStory);
		public Type RuntimeMacrosType = typeof(TwineRuntimeMacros);
		public bool StrictMode = false;
	}
}