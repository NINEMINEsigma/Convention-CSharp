using System;

namespace Convention.EasySave.Types
{
	public class EasySaveType_double : EasySaveType
	{
		public static EasySaveType Instance = null;

		public EasySaveType_double() : base(typeof(double))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, EasySaveWriter writer)
		{
			writer.WritePrimitive((double)obj);
		}

		public override object Read<T>(EasySaveReader reader)
		{
			return (T)(object)reader.Read_double();
		}
	}

	public class ES3Type_doubleArray : EasySaveArrayType
	{
		public static EasySaveType Instance;

		public ES3Type_doubleArray() : base(typeof(double[]), EasySaveType_double.Instance)
		{
			Instance = this;
		}
	}
}