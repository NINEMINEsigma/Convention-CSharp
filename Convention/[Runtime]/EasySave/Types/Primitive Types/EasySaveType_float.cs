using System;

namespace Convention.EasySave.Types
{
	public class EasySaveType_float : EasySaveType
	{
		public static EasySaveType Instance = null;

		public EasySaveType_float() : base(typeof(float))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, EasySaveWriter writer)
		{
			writer.WritePrimitive((float)obj);
		}

		public override object Read<T>(EasySaveReader reader)
		{
			return (T)(object)reader.Read_float();
		}
	}

	public class ES3Type_floatArray : EasySaveArrayType
	{
		public static EasySaveType Instance;

		public ES3Type_floatArray() : base(typeof(float[]), EasySaveType_float.Instance)
		{
			Instance = this;
		}
	}
}