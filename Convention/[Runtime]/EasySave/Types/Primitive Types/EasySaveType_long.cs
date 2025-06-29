using System;

namespace Convention.EasySave.Types
{
	public class EasySaveType_long : EasySaveType
	{
		public static EasySaveType Instance = null;

		public EasySaveType_long() : base(typeof(long))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, EasySaveWriter writer)
		{
			writer.WritePrimitive((long)obj);
		}

		public override object Read<T>(EasySaveReader reader)
		{
			return (T)(object)reader.Read_long();
		}
	}

	public class ES3Type_longArray : EasySaveArrayType
	{
		public static EasySaveType Instance;

		public ES3Type_longArray() : base(typeof(long[]), EasySaveType_long.Instance)
		{
			Instance = this;
		}
	}
}