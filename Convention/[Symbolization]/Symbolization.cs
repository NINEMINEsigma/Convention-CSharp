using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convention.Symbolization
{
    public sealed class SymbolizationContext
    {
        public SymbolizationContext() { }
        public SymbolizationContext(SymbolizationContext forward)
        {
            ParentContext = forward;
        }

        private readonly SymbolizationContext ParentContext;

        public void Execute(string commandsBlock)
        {

        }
    }

    public sealed class SymbolizationCore
    {
        internal SymbolizationCore(Internal.Reader reader)
        {

        }

        public SymbolizationContext Context { get; private set; }

    }

    public static class Symbolization
    {
        public static SymbolizationCore Build(string file)
        {
            return new SymbolizationCore(new(file));
        }

        public static SymbolizationContext Execute(string commandsBlock)
        {
            SymbolizationContext context = new();
            context.Execute(commandsBlock);
            return context;
        }

        public static SymbolizationContext Execute(SymbolizationContext forward,string commandsBlock)
        {
            SymbolizationContext context = new(forward);
            context.Execute(commandsBlock);
            return context;
        }
    }
}
