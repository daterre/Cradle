using System;

namespace Cradle
{
	[Serializable]
	public class StoryException : Exception
	{
		public StoryException() { }
		public StoryException(string message) : base(message) { }
		public StoryException(string message, Exception inner) : base(message, inner) { }
		protected StoryException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class VarTypeException : StoryException
	{
		public VarTypeException() { }
		public VarTypeException(string message) : base(message) { }
		public VarTypeException(string message, Exception inner) : base(message, inner) { }
		protected VarTypeException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class MacroException : StoryException
	{
		public MacroException() { }
		public MacroException(string message) : base(message) { }
		public MacroException(string message, Exception inner) : base(message, inner) { }
		protected MacroException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class VarTypeMemberException : StoryException
	{
		public VarTypeMemberException() { }
		public VarTypeMemberException(string message) : base(message) { }
		public VarTypeMemberException(string message, Exception inner) : base(message, inner) { }
		protected VarTypeMemberException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class StrictModeException : StoryException
	{
		public StrictModeException() { }
		public StrictModeException(string message) : base(message) { }
		public StrictModeException(string message, Exception inner) : base(message, inner) { }
		protected StrictModeException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}