using System;

namespace Convention.RScript
{
	[Serializable]
	public class RScriptExceptionException : Exception
	{
		public RScriptExceptionException(string message, int runtimePointer) : base($"when running {runtimePointer}, {message}") { }
		public RScriptExceptionException(string message, int runtimePointer, Exception inner) : base($"when running {runtimePointer}, {message}", inner) { }
	}
}
