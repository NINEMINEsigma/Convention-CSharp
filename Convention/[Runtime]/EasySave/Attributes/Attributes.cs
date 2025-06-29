using System;

namespace Convention.EasySave
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
    public class EasySaved : Attribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
    public class EasySaveIgnored : Attribute { }
}