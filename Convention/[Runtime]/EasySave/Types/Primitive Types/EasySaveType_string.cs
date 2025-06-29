using System;

namespace Convention.EasySave.Types
{
	public class EasySaveType_string : EasySaveType
	{
		public static EasySaveType Instance = null;

		public EasySaveType_string() : base(typeof(string))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, EasySaveWriter writer)
		{
			writer.WritePrimitive((string)obj);
		}

		public override object Read<T>(EasySaveReader reader)
		{
			return reader.Read_string();
		}
	}

	public class ES3Type_StringArray : EasySaveArrayType
	{
		public static EasySaveType Instance;

		public ES3Type_StringArray() : base(typeof(string[]), EasySaveType_string.Instance)
		{
			Instance = this;
		}
	}
}