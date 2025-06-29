using System;
using System.Collections.Generic;

namespace Convention.EasySave.Types
{
	public class EasySaveType_EasySaveRef : EasySaveType
	{
		public static EasySaveType Instance = new EasySaveType_EasySaveRef();

		public EasySaveType_EasySaveRef() : base(typeof(long))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, EasySaveWriter writer)
		{
			writer.WritePrimitive(((long)obj).ToString());
		}

		public override object Read<T>(EasySaveReader reader)
		{
			return (T)(object)new ES3Ref(reader.Read_ref());
		}
	}

	public class ES3Type_ES3RefArray : EasySaveArrayType
	{
		public static EasySaveType Instance = new ES3Type_ES3RefArray();

		public ES3Type_ES3RefArray() : base(typeof(ES3Ref[]), EasySaveType_EasySaveRef.Instance)
		{
			Instance = this;
		}
	}

    public class ES3Type_ES3RefDictionary : EasySaveDictionaryType
    {
        public static EasySaveType Instance = new ES3Type_ES3RefDictionary();

        public ES3Type_ES3RefDictionary() : base(typeof(Dictionary<ES3Ref, ES3Ref>), EasySaveType_EasySaveRef.Instance, EasySaveType_EasySaveRef.Instance)
        {
            Instance = this;
        }
    }
}

public class ES3Ref
{
    public long id;
    public ES3Ref(long id)
    {
        this.id = id;
    }
}