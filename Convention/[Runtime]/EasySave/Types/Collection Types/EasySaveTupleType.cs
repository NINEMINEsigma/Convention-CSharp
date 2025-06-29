using System;
using System.Collections;
using System.Collections.Generic;
using Convention.EasySave.Internal;

namespace Convention.EasySave.Types
{
	public class EasySaveTupleType : EasySaveType
	{
        public EasySaveType[] es3Types;
        public Type[] types;

		protected EasySaveReflection.EasySaveReflectedMethod readMethod = null;
		protected EasySaveReflection.EasySaveReflectedMethod readIntoMethod = null;

		public EasySaveTupleType(Type type) : base(type)
		{
			types = EasySaveReflection.GetElementTypes(type);
            es3Types = new EasySaveType[types.Length];

            for(int i=0; i<types.Length; i++)
            {
                es3Types[i] = EasySaveTypeMgr.GetOrCreateEasySaveType(types[i], false);
                if (es3Types[i] == null)
                    isUnsupported = true;
            }

			isTuple = true;
		}

        public override void Write(object obj, EasySaveWriter writer)
		{
			Write(obj, writer, writer.settings.memberReferenceMode);
		}

		public void Write(object obj, EasySaveWriter writer, EasySave.ReferenceMode memberReferenceMode)
		{
            if (obj == null) { writer.WriteNull(); return; };

            writer.StartWriteCollection();

            for (int i=0; i<es3Types.Length; i++)
            {
                var itemProperty = EasySaveReflection.GetProperty(type, "Item"+(i+1));
                var item = itemProperty.GetValue(obj);
                writer.StartWriteCollectionItem(i);
                writer.Write(item, es3Types[i], memberReferenceMode);
                writer.EndWriteCollectionItem(i);
            }

            writer.EndWriteCollection();
		}

        public override object Read<T>(EasySaveReader reader)
        {
            var objects = new object[types.Length];
            
            if (reader.StartReadCollection())
                return null;

            for(int i=0; i<types.Length; i++)
            {
                reader.StartReadCollectionItem();
                objects[i] = reader.Read<object>(es3Types[i]);
                reader.EndReadCollectionItem();
            }

            reader.EndReadCollection();

            var constructor = type.GetConstructor(types);
            var instance = constructor.Invoke(objects);

            return instance;
        }
    }
}