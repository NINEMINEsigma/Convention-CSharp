using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convention.Symbolization.Internal
{
    public abstract class Modification : Variable
    {
        protected Modification(string modificationalName, Type type) : base(new(modificationalName, type))
        {
        }
    }
    public abstract class Modification<T> : Modification where T : Modification<T>
    {
        protected Modification(string modificationalName) : base(modificationalName, typeof(T))
        {
        }
    }
}
