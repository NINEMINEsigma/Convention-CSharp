using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convention.Symbolization
{
    public class SymbolizationContext
    {

    }

    public class SymbolizationRunner
    {
        private readonly Internal.Namespace GlobalNamespace;
        private readonly SymbolizationContext Context;

        public SymbolizationRunner(SymbolizationContext context)
        {
            GlobalNamespace = new();
            Context = context;
        }
        public SymbolizationRunner() :this(new()){ }

    }
}
