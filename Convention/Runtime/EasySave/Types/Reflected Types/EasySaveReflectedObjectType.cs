using System;
using System.Collections;
using System.Collections.Generic;
using Convention.EasySave.Internal;

namespace Convention.EasySave.Types
{
	internal class EasySaveReflectedObjectType : EasySaveObjectType
	{
		public EasySaveReflectedObjectType(Type type) : base(type)
		{
			isReflectedType = true;
			GetMembers(true);
		}

		protected override void WriteObject(object obj, EasySaveWriter writer)
		{
			WriteProperties(obj, writer);
        }

		protected override object ReadObject<T>(EasySaveReader reader)
		{
			var obj = EasySaveReflection.CreateInstance(this.type);
			ReadProperties(reader, obj);
			return obj;
		}

		protected override void ReadObject<T>(EasySaveReader reader, object obj)
		{
			ReadProperties(reader, obj);
		}
	}
}