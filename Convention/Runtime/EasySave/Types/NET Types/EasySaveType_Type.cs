using System;

namespace Convention.EasySave.Types
{
    [EasySaveProperties()]
    public class EasySaveType_Type : EasySaveType
    {
        public static EasySaveType Instance = null;

        public EasySaveType_Type() : base(typeof(System.Type))
        {
            Instance = this;
        }

        public override void Write(object obj, EasySaveWriter writer)
        {
            Type type = (Type)obj;
            writer.WriteProperty("assemblyQualifiedName", type.AssemblyQualifiedName);
        }

        public override object Read<T>(EasySaveReader reader)
        {
            return Type.GetType(reader.ReadProperty<string>());
        }
    }
}