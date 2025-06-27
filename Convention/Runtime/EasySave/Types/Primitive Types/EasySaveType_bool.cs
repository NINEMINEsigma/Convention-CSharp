using System;

namespace Convention.EasySave.Types
{
	public class EasySaveType_bool : EasySaveType
	{
		public static EasySaveType Instance = null;

		public EasySaveType_bool() : base(typeof(bool))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, EasySaveWriter writer)
		{
			writer.WritePrimitive((bool)obj);
		}

		public override object Read<T>(EasySaveReader reader)
		{
			return (T)(object)reader.Read_bool();
		}
	}

	public class ES3Type_boolArray : EasySaveArrayType
	{
		public static EasySaveType Instance;

		public ES3Type_boolArray() : base(typeof(bool[]), EasySaveType_bool.Instance)
		{
			Instance = this;
		}
	}
}