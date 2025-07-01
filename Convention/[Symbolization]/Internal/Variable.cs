using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convention.Symbolization.Internal
{
    public abstract class Variable : ICloneable, IEquatable<Variable>
    {
        public readonly VariableSymbol SymbolInfo;

        /// <summary>
        /// for construct
        /// </summary>
        protected Variable(string symbolName, Type variableType)
        {
            SymbolInfo = new VariableSymbol(symbolName, variableType);
        }
        /// <summary>
        /// for clone
        /// </summary>
        protected Variable(string symbolName, Variable variable) : this(symbolName, variable.SymbolInfo.VariableType) { }

        public abstract object Clone();
        public abstract bool Equals(Variable other);

        public override bool Equals(object obj)
        {
            return obj is Variable variable && Equals(variable);
        }

        public abstract int GetVariableHashCode();

        public override sealed int GetHashCode()
        {
            return GetVariableHashCode();
        }
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
