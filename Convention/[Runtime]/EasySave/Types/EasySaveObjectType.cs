using System;
using System.Collections;
using System.Collections.Generic;
using Convention.EasySave.Internal;

namespace Convention.EasySave.Types
{
	public abstract class EasySaveObjectType : EasySaveType
	{
		public EasySaveObjectType(Type type) : base(type) {}

		protected abstract void WriteObject(object obj, EasySaveWriter writer);
		protected abstract object ReadObject<T>(EasySaveReader reader);

		protected virtual void ReadObject<T>(EasySaveReader reader, object obj)
		{
			throw new NotSupportedException("ReadInto is not supported for type "+type);
		}

		public override void Write(object obj, EasySaveWriter writer)
		{
            if (!WriteUsingDerivedType(obj, writer))
            {
                var baseType = EasySaveReflection.BaseType(obj.GetType());
                if (baseType != typeof(object))
                {
                    var es3Type = EasySaveTypeMgr.GetOrCreateEasySaveType(baseType, false);
                    // If it's a Dictionary or Collection, we need to write it as a field with a property name.
                    if (es3Type != null && (es3Type.isDictionary || es3Type.isCollection))
                        writer.WriteProperty("_Values", obj, es3Type);
                }

                WriteObject(obj, writer);
            }
        }

		public override object Read<T>(EasySaveReader reader)
		{
			string propertyName;
			while(true)
			{
				propertyName = ReadPropertyName(reader);

				if(propertyName == EasySaveType.typeFieldName)
					return EasySaveTypeMgr.GetOrCreateEasySaveType(reader.ReadType()).Read<T>(reader);
				else
				{
					reader.overridePropertiesName = propertyName;

					return ReadObject<T>(reader);
				}
			}
		}

		public override void ReadInto<T>(EasySaveReader reader, object obj)
		{
			string propertyName;
			while(true)
			{
				propertyName = ReadPropertyName(reader);

				if(propertyName == EasySaveType.typeFieldName)
				{
					EasySaveTypeMgr.GetOrCreateEasySaveType(reader.ReadType()).ReadInto<T>(reader, obj);
					return;
				}
                // This is important we return if the enumerator returns null, otherwise we will encounter an endless cycle.
                else if (propertyName == null)
					return;
				else
				{
					reader.overridePropertiesName = propertyName;
					ReadObject<T>(reader, obj);
				}
			}
		}
	}
}