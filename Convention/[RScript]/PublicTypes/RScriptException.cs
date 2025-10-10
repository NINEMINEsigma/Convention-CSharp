using System;

namespace Convention.RScript
{
	[Serializable]
	public class RScriptException : Exception
	{
		public RScriptException(string message, int runtimePointer) : base($"when running {runtimePointer}, {message}") { }
		public RScriptException(string message, int runtimePointer, Exception inner) : base($"when running {runtimePointer}, {message}", inner) { }
	}
}
