using System;

namespace Convention.EasySave.Types
{
	public class EasySaveType_byteArray : EasySaveType
	{
		public static EasySaveType Instance = null;

		public EasySaveType_byteArray() : base(typeof(byte[]))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, EasySaveWriter writer)
		{
			writer.WritePrimitive((byte[])obj);
		}

		public override object Read<T>(EasySaveReader reader)
		{
			return (T)(object)reader.Read_byteArray();
		}
	}
}