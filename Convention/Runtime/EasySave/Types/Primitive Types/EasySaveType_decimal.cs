using System;

namespace Convention.EasySave.Types
{ 
	public class EasySaveType_decimal : EasySaveType
	{
		public static EasySaveType Instance = null;

		public EasySaveType_decimal() : base(typeof(decimal))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, EasySaveWriter writer)
		{
			writer.WritePrimitive((decimal)obj);
		}

		public override object Read<T>(EasySaveReader reader)
		{
			return (T)(object)reader.Read_decimal();
		}
	}

	public class ES3Type_decimalArray : EasySaveArrayType
	{
		public static EasySaveType Instance;

		public ES3Type_decimalArray() : base(typeof(decimal[]), EasySaveType_decimal.Instance)
		{
			Instance = this;
		}
	}
}