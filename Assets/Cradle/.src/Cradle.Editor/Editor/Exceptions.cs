using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cradle.Editor
{
	public class StoryImportException : Exception
	{
		public StoryImportException() { }
		public StoryImportException(string message) : base(message) { }
		public StoryImportException(string message, Exception inner) : base(message, inner) { }
	}

	public class StoryFormatTranscodeException : Exception
	{
		public string Passage;
		public StoryFormatTranscodeException() { }
		public StoryFormatTranscodeException(string message, string passageName = null) : base(message)
		{
			Passage = passageName;
		}
		public StoryFormatTranscodeException(string message, Exception inner, string passageName = null)
			: base(message, inner)
		{
			Passage = passageName;
		}
	}
}
