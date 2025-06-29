using System;

namespace Convention.EasySave.Types
{
	public class EasySaveType_uint : EasySaveType
	{
		public static EasySaveType Instance = null;

		public EasySaveType_uint() : base(typeof(uint))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, EasySaveWriter writer)
		{
			writer.WritePrimitive((uint)obj);
		}

		public override object Read<T>(EasySaveReader reader)
		{
			return (T)(object)reader.Read_uint();
		}
	}

	public class ES3Type_uintArray : EasySaveArrayType
	{
		public static EasySaveType Instance;

		public ES3Type_uintArray() : base(typeof(uint[]), EasySaveType_uint.Instance)
		{
			Instance = this;
		}
	}
}