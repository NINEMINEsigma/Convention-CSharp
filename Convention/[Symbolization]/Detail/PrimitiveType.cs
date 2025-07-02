using System;

namespace Convention.Symbolization.Primitive
{
    public class PrimitiveType<T> : Internal.Variable
    {
        public PrimitiveType() : base(new(typeof(T).Name, typeof(T)))
        {
        }

        public override bool Equals(Internal.Variable other)
        {
            return other is PrimitiveType<T>;
        }

        public virtual T CloneValue(T value)
        {
            if (Utility.IsNumber(typeof(T)))
                return value;
            else if (Utility.IsString(typeof(T)))
                return (T)(object)new string((string)(object)value);
            else if (value is ICloneable cloneable)
                return (T)cloneable.Clone();
            else if (value is Internal.Variable)
                return value;
            return value;
        }

        public virtual T DefaultValue() => default;
    }

    public class PrimitiveInstance<T> : Internal.CloneableVariable<PrimitiveInstance<T>>
    {
        private readonly PrimitiveType<T> MyPrimitiveType = new();
        public T Value;

        public PrimitiveInstance(string symbolName, T value, PrimitiveType<T> primitiveType) : base(symbolName)
        {
            this.Value = value;
            this.MyPrimitiveType = primitiveType;
        }

        public override PrimitiveInstance<T> CloneVariable(string targetSymbolName)
        {
            return new(targetSymbolName, MyPrimitiveType.CloneValue(this.Value), this.MyPrimitiveType);
        }

        public override bool Equals(PrimitiveInstance<T> other)
        {
            return this.Value.Equals(other.Value);
        }
        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
