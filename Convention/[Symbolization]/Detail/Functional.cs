using System;
using System.Linq;
using System.Reflection;

namespace Convention.Symbolization.Internal
{
    public abstract class Function : CloneableVariable<Function>
    {
        public readonly FunctionSymbol FunctionInfo;

        protected Function(string symbolName,string functionName, Type returnType, Type[] parameterTypes, Modification[] modifiers) : base(symbolName)
        {
            FunctionInfo = new(functionName, returnType, parameterTypes, modifiers);
        }
        public abstract Variable Invoke(SymbolizationContext context, Variable[] parameters);
    }

    public sealed class DelegationalFunction : Function
    {
        public readonly MethodInfo methodInfo;

        public DelegationalFunction(string symbolName, MethodInfo methodInfo)
            : base(symbolName, methodInfo.Name, methodInfo.ReturnType, methodInfo.GetParameters().ToList().ConvertAll(x => x.ParameterType).ToArray(), Array.Empty<Modification>())
        {
            this.methodInfo = methodInfo!;
        }

        public override DelegationalFunction CloneVariable(string targetSymbolName)
        {
            return new DelegationalFunction(targetSymbolName, methodInfo);
        }

        public override bool Equals(Function other)
        {
            return other is DelegationalFunction df && df.methodInfo == this.methodInfo;
        }

        public override Variable Invoke(SymbolizationContext context, Variable[] parameters)
        {
            Variable invoker = parameters.Length > 0 ? parameters[0] : null;
            Variable[] invokerParameters = parameters.Length > 0 ? parameters.Skip(1).ToArray() : Array.Empty<Variable>();
            object result = methodInfo.Invoke(invoker, invokerParameters);
            return result as Variable;
        }
    }


    public readonly struct FunctionSymbol
    {
        public readonly string FunctionName;
        public readonly Type ReturnType;
        public readonly Type[] ParameterTypes;
        public readonly Modification[] Modifiers;
        public readonly string FullName;

        public FunctionSymbol(string symbolName, Type returnType, Type[] parameterTypes, Modification[] modifiers)
        {
            this.FunctionName = symbolName;
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
