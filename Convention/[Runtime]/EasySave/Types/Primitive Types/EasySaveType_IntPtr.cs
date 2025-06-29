using System;

namespace Convention.EasySave.Types
{
	public class EasySaveType_IntPtr : EasySaveType
	{
		public static EasySaveType Instance = null;

		public EasySaveType_IntPtr() : base(typeof(IntPtr))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, EasySaveWriter writer)
		{
			writer.WritePrimitive((long)(IntPtr)obj);
		}

		public override object Read<T>(EasySaveReader reader)
		{
			return (T)(object)(IntPtr)reader.Read_long();
		}
	}

	public class ES3Type_IntPtrArray : EasySaveArrayType
	{
		public static EasySaveType Instance;

		public ES3Type_IntPtrArray() : base(typeof(IntPtr[]), EasySaveType_IntPtr.Instance)
		{
			Instance = this;
		}
	}
}