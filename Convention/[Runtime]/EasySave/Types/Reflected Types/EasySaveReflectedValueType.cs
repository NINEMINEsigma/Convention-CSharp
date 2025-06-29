using System;
using System.Collections;
using System.Collections.Generic;
using Convention.EasySave.Internal;

namespace Convention.EasySave.Types
{
	internal class EasySaveReflectedValueType : EasySaveType
	{
		public EasySaveReflectedValueType(Type type) : base(type)
		{
			isReflectedType = true;
			GetMembers(true);
		}

		public override void Write(object obj, EasySaveWriter writer)
		{
			WriteProperties(obj, writer);
		}

		public override object Read<T>(EasySaveReader reader)
		{
			var obj = EasySaveReflection.CreateInstance(this.type);

			if(obj == null)
				throw new NotSupportedException("Cannot create an instance of "+this.type+". However, you may be able to add support for it using a custom EasySaveType file. For more information see: http://docs.moodkie.com/easy-save-3/es3-guides/controlling-serialization-using-es3types/");
			// Make sure we return the result of ReadProperties as properties aren't assigned by reference.
			return ReadProperties(reader, obj);
		}

		public override void ReadInto<T>(EasySaveReader reader, object obj)
		{
			throw new NotSupportedException("Cannot perform self-assigning load on a value type.");
		}
	}
}