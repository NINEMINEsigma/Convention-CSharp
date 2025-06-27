using System;
using System.Collections;
using System.Collections.Generic;
using Convention.EasySave.Internal;

namespace Convention.EasySave.Types
{
	public class EasySaveDictionaryType : EasySaveType
	{
		public EasySaveType keyType;
		public EasySaveType valueType;

		protected EasySaveReflection.EasySaveReflectedMethod readMethod = null;
		protected EasySaveReflection.EasySaveReflectedMethod readIntoMethod = null;

		public EasySaveDictionaryType(Type type) : base(type)
		{
			var types = EasySaveReflection.GetElementTypes(type);
			keyType = EasySaveTypeMgr.GetOrCreateEasySaveType(types[0], false);
			valueType = EasySaveTypeMgr.GetOrCreateEasySaveType(types[1], false);

			// If either the key or value type is unsupported, make this type NULL.
			if(keyType == null || valueType == null)
				isUnsupported = true;;

			isDictionary = true;
		}

        public EasySaveDictionaryType(Type type, EasySaveType keyType, EasySaveType valueType) : base(type)
        {
            this.keyType = keyType;
            this.valueType = valueType;

            // If either the key or value type is unsupported, make this type NULL.
            if (keyType == null || valueType == null)
                isUnsupported = true; ;

            isDictionary = true;
        }

        public override void Write(object obj, EasySaveWriter writer)
		{
			Write(obj, writer, writer.settings.memberReferenceMode);
		}

		public void Write(object obj, EasySaveWriter writer, EasySave.ReferenceMode memberReferenceMode)
		{
			var dict = (IDictionary)obj;

			//writer.StartWriteDictionary(dict.Count);

			int i=0;
			foreach(System.Collections.DictionaryEntry kvp in dict)
			{
				writer.StartWriteDictionaryKey(i);
				writer.Write(kvp.Key, keyType, memberReferenceMode);
				writer.EndWriteDictionaryKey(i);
				writer.StartWriteDictionaryValue(i);
				writer.Write(kvp.Value, valueType, memberReferenceMode);
				writer.EndWriteDictionaryValue(i);
				i++;
			}

			//writer.EndWriteDictionary();
		}

		public override object Read<T>(EasySaveReader reader)
		{
			return Read(reader);
		}

		public override void ReadInto<T>(EasySaveReader reader, object obj)
		{
            ReadInto(reader, obj);
		}

		/*
		 * 	Allows us to call the generic Read method using Reflection so we can define the generic parameter at runtime.
		 * 	It also caches the method to improve performance in later calls.
		 */
		public object Read(EasySaveReader reader)
		{
			if(reader.StartReadDictionary())
				return null;

			var dict = (IDictionary)EasySaveReflection.CreateInstance(type);

			// Iterate through each character until we reach the end of the array.
			while(true)
			{
				if(!reader.StartReadDictionaryKey())
					return dict;
				var key = reader.Read<object>(keyType);
				reader.EndReadDictionaryKey();

				reader.StartReadDictionaryValue();
				var value = reader.Read<object>(valueType);

				dict.Add(key,value);

				if(reader.EndReadDictionaryValue())
					break;
			}

			reader.EndReadDictionary();

			return dict;
		}

		public void ReadInto(EasySaveReader reader, object obj)
		{
			if(reader.StartReadDictionary())
				throw new NullReferenceException("The Dictionary we are trying to load is stored as null, which is not allowed when using ReadInto methods.");

			var dict = (IDictionary)obj;

			// Iterate through each character until we reach the end of the array.
			while(true)
			{
				if(!reader.StartReadDictionaryKey())
					return;
				var key = reader.Read<object>(keyType);

				if(!dict.Contains(key))
					throw new KeyNotFoundException("The key \"" + key + "\" in the Dictionary we are loading does not exist in the Dictionary we are loading into");
				var value = dict[key];
				reader.EndReadDictionaryKey();

				reader.StartReadDictionaryValue();

				reader.ReadInto<object>(value, valueType);

				if(reader.EndReadDictionaryValue())
					break;
			}

			reader.EndReadDictionary();
		}
	}
}