using System;

namespace Convention.EasySave.Types
{
	public class EasySaveType_UIntPtr : EasySaveType
	{
		public static EasySaveType Instance = null;

		public EasySaveType_UIntPtr() : base(typeof(UIntPtr))
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
			return (object)reader.Read_ulong();
		}
	}

	public class ES3Type_UIntPtrArray : EasySaveArrayType
	{
		public static EasySaveType Instance;

		public ES3Type_UIntPtrArray() : base(typeof(UIntPtr[]), EasySaveType_UIntPtr.Instance)
		{
			Instance = this;
		}
	}
}