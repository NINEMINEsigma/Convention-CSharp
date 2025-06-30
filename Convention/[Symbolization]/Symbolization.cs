using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convention.Symbolization
{
    public class SymbolizationContext
    {
        public Internal.VariableContext Context = new();
    }

    public class SymbolizationRunner
    {
        private readonly SymbolizationContext Context;

        public SymbolizationRunner(SymbolizationContext context)
        {
            Context = context;
        }
        public SymbolizationRunner() :this(new()){ }

        public void Execute(string funcName)
        {

        }

        public void Execute()=>Execute("main")
    }
}
