using System;
using System.Collections;
using System.Collections.Generic;
using Convention.EasySave.Internal;
using System.Linq;
using System.Reflection;

namespace Convention.EasySave.Types
{
	public class EasySaveHashSetType : ECollectionType
	{
		public EasySaveHashSetType(Type type) : base(type){}

        public override void Write(object obj, EasySaveWriter writer, EasySave.ReferenceMode memberReferenceMode)
        {
            if (obj == null) { writer.WriteNull(); return; };

            var list = (IEnumerable)obj;

            if (elementType == null)
                throw new ArgumentNullException("EasySaveType argument cannot be null.");

            int count = 0;
            foreach (var item in list)
                count++;

            //writer.StartWriteCollection(count);

            int i = 0;
            foreach (object item in list)
            {
                writer.StartWriteCollectionItem(i);
                writer.Write(item, elementType, memberReferenceMode);
                writer.EndWriteCollectionItem(i);
                i++;
            }

            //writer.EndWriteCollection();
        }

        public override object Read<T>(EasySaveReader reader)
        {
            var val = Read(reader);
            if (val == null)
                return default(T);
            return (T)val;
        }


        public override object Read(EasySaveReader reader)
		{
            /*var method = typeof(ECollectionType).GetMethod("ReadICollection", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(elementType.type);
            if(!(bool)method.Invoke(this, new object[] { reader, list, elementType }))
                return null;*/

            var genericParam = EasySaveReflection.GetGenericArguments(type)[0];
            var listType = EasySaveReflection.MakeGenericType(typeof(List<>), genericParam);
            var list = (IList)EasySaveReflection.CreateInstance(listType);

            if (!reader.StartReadCollection())
            {
                // Iterate through each character until we reach the end of the array.
                while (true)
                {
                    if (!reader.StartReadCollectionItem())
                        break;
                    list.Add(reader.Read<object>(elementType));

                    if (reader.EndReadCollectionItem())
                        break;
                }

                reader.EndReadCollection();
            }

            return EasySaveReflection.CreateInstance(type, list);
        }

        public override void ReadInto<T>(EasySaveReader reader, object obj)
        {
            ReadInto(reader, obj);
        }

        public override void ReadInto(EasySaveReader reader, object obj)
		{
            throw new NotImplementedException("Cannot use LoadInto/ReadInto with HashSet because HashSets do not maintain the order of elements");
		}
    }
}