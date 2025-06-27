using System;

namespace Convention.EasySave.Types
{
	public class EasySaveType_ulong : EasySaveType
	{
		public static EasySaveType Instance = null;

		public EasySaveType_ulong() : base(typeof(ulong))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, EasySaveWriter writer)
		{
			writer.WritePrimitive((ulong)obj);
		}

		public override object Read<T>(EasySaveReader reader)
		{
			return (T)(object)reader.Read_ulong();
		}
	}

	public class ES3Type_ulongArray : EasySaveArrayType
	{
		public static EasySaveType Instance;

		public ES3Type_ulongArray() : base(typeof(ulong[]), EasySaveType_ulong.Instance)
		{
			Instance = this;
		}
	}
}