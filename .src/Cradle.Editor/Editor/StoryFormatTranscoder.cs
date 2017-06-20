using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace Cradle.Editor
{
	public abstract class StoryFormatTranscoder
	{
		public StoryImporter Importer { get; internal set; }

		public abstract StoryFormatMetadata GetMetadata();
		public virtual void Init() { }
		public abstract PassageCode PassageToCode(PassageData passage);
		public virtual bool RecognizeFormat() { return true; }

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
		public string StartPassage;
		public string StoryFormatName;
		public Type StoryBaseType = typeof(Story);
		public bool StrictMode = false;
	}

	public class GeneratedCode
	{
		public StringBuilder Buffer = new StringBuilder();
		public int Indentation = 0;
		public bool Collapsed = false;

		public void Indent()
		{
			Utils.CodeGenUtils.Indent(Indentation, Buffer);
		}
	}
}