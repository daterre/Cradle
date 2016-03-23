using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityTwine.Editor
{
	[Serializable]
	public class TwineImportException : Exception
	{
		public TwineImportException() { }
		public TwineImportException(string message) : base(message) { }
		public TwineImportException(string message, Exception inner) : base(message, inner) { }
		protected TwineImportException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
