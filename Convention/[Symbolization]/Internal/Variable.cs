using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convention.Symbolization.Internal
{
    public abstract class Variable : ICloneable
    {
        public abstract object Clone();
    }

    public readonly struct VariableSymbol
    {
        public readonly string SymbolName;
        public readonly Type VariableType;

        public VariableSymbol(string symbolName, Type variableType)
        {
            this.SymbolName = symbolName;
            this.VariableType = variableType;
        }

        public bool Equals(VariableSymbol other)
        {
            return other.SymbolName.Equals(SymbolName) && other.VariableType.Equals(VariableType);
        }

        public override int GetHashCode()
        {
            return new Tuple<string, Type>(SymbolName, VariableType).GetHashCode();
        }
    }

    public class VariableContext
    {
        public Dictionary<VariableSymbol, Variable> Variables = new();
    }
}
