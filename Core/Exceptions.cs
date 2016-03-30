using System;

namespace UnityTwine
{
	[Serializable]
	public class TwineException : Exception
	{
		public TwineException() { }
		public TwineException(string message) : base(message) { }
		public TwineException(string message, Exception inner) : base(message, inner) { }
		protected TwineException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class TwineStrictModeException : TwineException
	{
		public TwineStrictModeException() { }
		public TwineStrictModeException(string message) : base(message) { }
		public TwineStrictModeException(string message, Exception inner) : base(message, inner) { }
		protected TwineStrictModeException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}