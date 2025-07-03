using System;
using System.Text;
using System.Threading.Tasks;

namespace Convention.Symbolization
{
    public class SymbolizationRunner
    {
        private SymbolizationContext Context;

        public void Compile(string path)
        {
            Context = new();
            Context.Compile(new ToolFile(path));
        }

    }
}
