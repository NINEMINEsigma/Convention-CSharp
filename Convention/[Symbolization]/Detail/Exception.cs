using System;

namespace Convention.Symbolization
{

	[Serializable]
	public class InvalidGrammarException : Exception
	{
		public InvalidGrammarException() { }
		public InvalidGrammarException(string message) : base(message) { }
		public InvalidGrammarException(string message, Exception inner) : base(message, inner) { }
		protected InvalidGrammarException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
