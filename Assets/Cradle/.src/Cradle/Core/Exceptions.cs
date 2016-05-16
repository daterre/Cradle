using System;

namespace Cradle
{
	[Serializable]
	public class CradleException : Exception
	{
		public CradleException() { }
		public CradleException(string message) : base(message) { }
		public CradleException(string message, Exception inner) : base(message, inner) { }
		protected CradleException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class VarTypeException : CradleException
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
	public class MacroException : CradleException
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
	public class VarTypeMemberException : CradleException
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
	public class StrictModeException : CradleException
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