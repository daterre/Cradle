using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityTwine.Editor
{
	public class TwineImportException : Exception
	{
		public TwineImportException() { }
		public TwineImportException(string message) : base(message) { }
		public TwineImportException(string message, Exception inner) : base(message, inner) { }
	}

	public class TwineTranscodingException : Exception
	{
		public string Passage;
		public TwineTranscodingException() { }
		public TwineTranscodingException(string message, string passageName = null) : base(message)
		{
			Passage = passageName;
		}
		public TwineTranscodingException(string message, Exception inner, string passageName = null)
			: base(message, inner)
		{
			Passage = passageName;
		}
	}
}
