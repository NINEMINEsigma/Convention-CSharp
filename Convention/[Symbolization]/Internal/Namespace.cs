using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convention.Symbolization.Internal
{
    internal sealed class NamespaceModel
    {

    }

    public sealed class Namespace : Variable
    {
        private NamespaceModel Data;

        public override object Clone()
        {
            Namespace result = new()
            {
                Data = Data
            };
            return result;
        }

        
    }
}
