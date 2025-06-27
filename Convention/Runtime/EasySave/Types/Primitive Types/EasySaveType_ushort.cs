using System;

namespace Convention.EasySave.Types
{
	public class EasySaveType_ushort : EasySaveType
	{
		public static EasySaveType Instance = null;

		public EasySaveType_ushort() : base(typeof(ushort))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, EasySaveWriter writer)
		{
			writer.WritePrimitive((ushort)obj);
		}

		public override object Read<T>(EasySaveReader reader)
		{
			return (T)(object)reader.Read_ushort();
		}
	}

	public class ES3Type_ushortArray : EasySaveArrayType
	{
		public static EasySaveType Instance;

		public ES3Type_ushortArray() : base(typeof(ushort[]), EasySaveType_ushort.Instance)
		{
			Instance = this;
		}
	}
}