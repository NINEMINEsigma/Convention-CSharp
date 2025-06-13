using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convention
{
    namespace SAL
    {
        public interface IHasCheckMethod
        {
            bool Check();
        }

        public class ReturnValue { }

        public enum ComparedFlag
        {
            Greater, Less, Equal, GreaterOrEqual, LessOrEqual, NotEqual
        }

        [System.AttributeUsage(AttributeTargets.ReturnValue, Inherited = true, AllowMultiple = true)]
        public sealed class SuccessAttribute : Attribute, IHasCheckMethod
        {
            private List<object> exprs = new();

            public bool Check()
            {
                throw new NotImplementedException();
            }
        }

    }

    public static class PlatformIndictaor
    {
    }
}
