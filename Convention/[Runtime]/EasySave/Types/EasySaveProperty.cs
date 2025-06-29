using System;
using System.ComponentModel;

namespace Convention.EasySave.Internal
{
	public class EasySaveMember
	{
		public string name;
		public Type type;
		public bool isProperty;
		public EasySaveReflection.EasySaveReflectedMember reflectedMember;
		public bool useReflection = false;

		public EasySaveMember(string name, Type type, bool isProperty)
		{
			this.name = name;
			this.type = type;
			this.isProperty = isProperty;
	 	}

		public EasySaveMember(EasySaveReflection.EasySaveReflectedMember reflectedMember)
		{
			this.reflectedMember = reflectedMember;
			this.name = reflectedMember.Name;
			this.type = reflectedMember.MemberType;
			this.isProperty = reflectedMember.isProperty;
			this.useReflection = true;
		}
	}
}
