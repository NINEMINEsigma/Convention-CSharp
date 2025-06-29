using System;

namespace Convention.EasySave.Types
{
	public class EasySaveType_int : EasySaveType
	{
		public static EasySaveType Instance = null;

		public EasySaveType_int() : base(typeof(int))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, EasySaveWriter writer)
		{
			writer.WritePrimitive((int)obj);
		}

		public override object Read<T>(EasySaveReader reader)
		{
			return (T)(object)reader.Read_int();
		}
	}

	public class ES3Type_intArray : EasySaveArrayType
	{
		public static EasySaveType Instance;

		public ES3Type_intArray() : base(typeof(int[]), EasySaveType_int.Instance)
		{
			Instance = this;
		}
	}
}