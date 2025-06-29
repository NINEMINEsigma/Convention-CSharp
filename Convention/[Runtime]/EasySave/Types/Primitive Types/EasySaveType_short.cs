using System;

namespace Convention.EasySave.Types
{
	public class EasySaveType_short : EasySaveType
	{
		public static EasySaveType Instance = null;

		public EasySaveType_short() : base(typeof(short))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, EasySaveWriter writer)
		{
			writer.WritePrimitive((short)obj);
		}

		public override object Read<T>(EasySaveReader reader)
		{
			return (T)(object)reader.Read_short();
		}
	}

	public class ES3Type_shortArray : EasySaveArrayType
	{
		public static EasySaveType Instance;

		public ES3Type_shortArray() : base(typeof(short[]), EasySaveType_short.Instance)
		{
			Instance = this;
		}
	}
}