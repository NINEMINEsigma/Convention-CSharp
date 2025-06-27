namespace Convention.EasySave.Types
{
	public class EasySaveType_char : EasySaveType
	{
		public static EasySaveType Instance = null;

		public EasySaveType_char() : base(typeof(char))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, EasySaveWriter writer)
		{
			writer.WritePrimitive((char)obj);
		}

		public override object Read<T>(EasySaveReader reader)
		{
			return (T)(object)reader.Read_char();
		}
	}
		public class ES3Type_charArray : EasySaveArrayType
		{
			public static EasySaveType Instance;

			public ES3Type_charArray() : base(typeof(char[]), EasySaveType_char.Instance)
			{
				Instance = this;
			}
	}
}