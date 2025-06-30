using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convention.Symbolization.Internal
{
    public class Function : Variable
    {
        public Variable Invoke(Variable[] parameters)
        {

        }
    }

    public readonly struct FunctionSymbol
    {
        public readonly string SymbolName;
        public readonly Type ReturnType;
        public readonly Type[] ParameterTypes;
        public readonly Modification[] Modifiers;
        public readonly string FullName;

        public FunctionSymbol(string symbolName, Type returnType, Type[] parameterTypes, Modification[] modifiers)
        {
            this.SymbolName = symbolName;
            this.ReturnType = returnType;
            this.ParameterTypes = parameterTypes;
            this.Modifiers = modifiers;
            this.FullName = $"{returnType.FullName} {symbolName}({string.Join(',', parameterTypes.ToList().ConvertAll(x => x.FullName))}) {string.Join(' ', modifiers.ToList().ConvertAll(x => x.ToString()))}";
        }

        public bool Equals(FunctionSymbol other)
        {
            return other.FullName.Equals(FullName);
        }

        public override int GetHashCode()
        {
            return FullName.GetHashCode();
        }
    }
}
