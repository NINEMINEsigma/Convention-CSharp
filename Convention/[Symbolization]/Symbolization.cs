using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convention.Symbolization
{
    public class SymbolizationContext
    {
        public SymbolizationContext() : this(null, Internal.Namespace.CreateRootNamespace()) { }
        public SymbolizationContext(SymbolizationContext parent) : this(parent, parent.CurrentNamespace) { }
        public SymbolizationContext(SymbolizationContext parent, Internal.Namespace newNamespace)
        {
            this.ParentContext = parent;
            this.CurrentNamespace = newNamespace;
        }

        public readonly SymbolizationContext ParentContext;
        public readonly Dictionary<string, Internal.Variable> Variables = new();
        public readonly Internal.Namespace CurrentNamespace;

        public SymbolizationContext Compile()
        {
            return this;
        }
    }

    public class SymbolizationRunner
    {
        private readonly Internal.Namespace GlobalNamespace;
        private readonly SymbolizationContext Context;

        public SymbolizationRunner(SymbolizationContext context)
        {
            GlobalNamespace = Internal.Namespace.CreateRootNamespace();
            Context = context;
        }
        public SymbolizationRunner() :this(new()){ }

        public Exception Compile()
        {
            try
            {

            }
            catch (Exception ex)
            {
                return ex;
            }
            return null;
        }

    }
}
