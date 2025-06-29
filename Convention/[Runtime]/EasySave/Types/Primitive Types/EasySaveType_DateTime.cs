using System;

namespace Convention.EasySave.Types
{
	public class EasySaveType_DateTime : EasySaveType
	{
		public static EasySaveType Instance = null;

		public EasySaveType_DateTime() : base(typeof(DateTime))
		{
			Instance = this;
		}

		public override void Write(object obj, EasySaveWriter writer)
		{
			writer.WriteProperty("ticks", ((DateTime)obj).Ticks, EasySaveType_long.Instance);
		}

		public override object Read<T>(EasySaveReader reader)
		{
			reader.ReadPropertyName();
			return new DateTime(reader.Read<long>(EasySaveType_long.Instance));
		}
	}

	public class ES3Type_DateTimeArray : EasySaveArrayType
	{
		public static EasySaveType Instance;

		public ES3Type_DateTimeArray() : base(typeof(DateTime[]), EasySaveType_DateTime.Instance)
		{
			Instance = this;
		}
	}
}