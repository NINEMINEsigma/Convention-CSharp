using System;

namespace Convention.Symbolization.Internal
{
    public abstract class Variable : IEquatable<Variable>
    {
        public readonly VariableSymbol SymbolInfo;

        protected Variable(VariableSymbol symbolInfo)
        {
            this.SymbolInfo = symbolInfo;
        }

        public abstract bool Equals(Variable other);

        public override bool Equals(object obj)
        {
            return Equals(obj as Variable);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }


    public abstract class Variable<T> : Variable, IEquatable<T>
    {
        protected Variable(string symbolName) : base(new VariableSymbol(symbolName, typeof(T))) { }
        public abstract bool Equals(T other);

        public override bool Equals(Variable other)
        {
            return other is T variable && Equals(variable);
        }

        public override bool Equals(object obj)
        {
            return obj is T variable && Equals(variable);
        }

        public virtual int GetVariableHashCode()
        {
            return base.GetHashCode();
        }

        public override sealed int GetHashCode()
        {
            return GetVariableHashCode();
        }
    }

    public interface ICanFindVariable
    {
        public Variable[] Find(string symbolName);
    }

    public abstract class CloneableVariable<T> : Variable<T>, ICloneable
    {
        protected CloneableVariable(string symbolName) : base(symbolName)
        {
        }

        public object Clone() => CloneVariable(SymbolInfo.SymbolName);

        public abstract T CloneVariable(string targetSymbolName);
    }

    public sealed class NullVariable : CloneableVariable<NullVariable>
    {
        public NullVariable(string symbolName) : base(symbolName)
        {
        }

        public override NullVariable CloneVariable(string targetSymbolName)
        {
            return new(targetSymbolName);
        }

        public override bool Equals(NullVariable other)
        {
            return true;
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
}
