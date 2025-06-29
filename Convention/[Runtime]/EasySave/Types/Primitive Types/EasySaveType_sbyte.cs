using System;

namespace Convention.EasySave.Types
{
	public class EasySaveType_sbyte : EasySaveType
	{
		public static EasySaveType Instance = null;

		public EasySaveType_sbyte() : base(typeof(sbyte))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, EasySaveWriter writer)
		{
			writer.WritePrimitive((sbyte)obj);
		}

		public override object Read<T>(EasySaveReader reader)
		{
			return (T)(object)reader.Read_sbyte();
		}
	}

		public class ES3Type_sbyteArray : EasySaveArrayType
	{
		public static EasySaveType Instance;

		public ES3Type_sbyteArray() : base(typeof(sbyte[]), EasySaveType_sbyte.Instance)
		{
			Instance = this;
		}
	}
}