using System;
using System.Collections;
using System.Collections.Generic;
using Convention.EasySave.Internal;
using System.Linq;

namespace Convention.EasySave.Types
{
	public class EasySaveNativeArrayType : ECollectionType
	{
		public EasySaveNativeArrayType(Type type) : base(type){}
		public EasySaveNativeArrayType(Type type, EasySaveType elementType) : base(type, elementType){}

		public override void Write(object obj, EasySaveWriter writer, EasySave.ReferenceMode memberReferenceMode)
		{
            if (elementType == null)
                throw new ArgumentNullException("EasySaveType argument cannot be null.");

            var enumerable = (IEnumerable)obj;

            int i = 0;
            foreach(var item in enumerable)
            {
                writer.StartWriteCollectionItem(i);
                writer.Write(item, elementType, memberReferenceMode);
                writer.EndWriteCollectionItem(i);
                i++;
            }
		}

        public override object Read(EasySaveReader reader)
        {
            var array = ReadAsArray(reader);

            return EasySaveReflection.CreateInstance(type, new object[] { array/*, Allocator.Persistent*/ });
        }

        public override object Read<T>(EasySaveReader reader)
		{
            return Read(reader);
		}

		public override void ReadInto<T>(EasySaveReader reader, object obj)
		{
            ReadInto(reader, obj);
		}

		public override void ReadInto(EasySaveReader reader, object obj)
		{
            var array = ReadAsArray(reader);
            var copyFromMethod = EasySaveReflection.GetMethods(type, "CopyFrom").First(m => EasySaveReflection.TypeIsArray(m.GetParameters()[0].GetType()));
            copyFromMethod.Invoke(obj, new object[] { array });
        }

        private System.Array ReadAsArray(EasySaveReader reader)
        {
            var list = new List<object>();
            if (!ReadICollection(reader, list, elementType))
                return null;

            var array = EasySaveReflection.ArrayCreateInstance(elementType.type, list.Count);
            int i = 0;
            foreach (var item in list)
            {
                array.SetValue(item, i);
                i++;
            }

            return array;
        }
	}
}