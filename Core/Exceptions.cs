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
	public class TwineTypeException : Exception
	{
		public TwineTypeException() { }
		public TwineTypeException(string message) : base(message) { }
		public TwineTypeException(string message, Exception inner) : base(message, inner) { }
		protected TwineTypeException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class TwineTypeMemberException : TwineException
	{
		public TwineTypeMemberException() { }
		public TwineTypeMemberException(string message) : base(message) { }
		public TwineTypeMemberException(string message, Exception inner) : base(message, inner) { }
		protected TwineTypeMemberException(
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