using System;
using System.Linq;
using System.Reflection;

namespace Convention.Symbolization.Internal
{
    public abstract class Function : CloneableVariable<Function>
    {
        public readonly FunctionSymbol FunctionInfo;

        protected Function(string symbolName,int lineIndex,int wordIndex,string functionName, Type returnType, Type[] parameterTypes) : base(symbolName, lineIndex, wordIndex)
        {
            FunctionInfo = new(functionName, returnType, parameterTypes);
        }
        public abstract Variable Invoke(SymbolizationContext context, Variable[] parameters);
    }

    public sealed class DelegationalFunction : Function
    {
        public readonly MethodInfo methodInfo;

        public DelegationalFunction(string symbolName, int lineIndex, int wordIndex, MethodInfo methodInfo)
            : base(symbolName, lineIndex, wordIndex,
                   methodInfo.Name, methodInfo.ReturnType, methodInfo.GetParameters().ToList().ConvertAll(x => x.ParameterType).ToArray())
        {
            this.methodInfo = methodInfo!;
        }

        public override DelegationalFunction CloneVariable(string targetSymbolName, int lineIndex, int wordIndex)
        {
            return new DelegationalFunction(targetSymbolName, lineIndex, wordIndex, methodInfo);
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

    public sealed class ScriptFunction : Function
    {
        public ScriptFunction(string symbolName, int lineIndex, int wordIndex,
            string functionName, Type returnType, Type[] parameterTypes)
            : base(symbolName, lineIndex, wordIndex, functionName, returnType, parameterTypes)
        {
        }

        public override Function CloneVariable(string targetSymbolName, int lineIndex, int wordIndex)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(Function other)
        {
            throw new NotImplementedException();
        }

        public override Variable Invoke(SymbolizationContext context, Variable[] parameters)
        {
            throw new NotImplementedException();
        }
    }

    public readonly struct FunctionSymbol
    {
        public readonly string FunctionName;
        public readonly Type ReturnType;
        public readonly Type[] ParameterTypes;
        public readonly string FullName;

        public FunctionSymbol(string symbolName, Type returnType, Type[] parameterTypes)
        {
            this.FunctionName = symbolName;
            this.ReturnType = returnType;
            this.ParameterTypes = parameterTypes;
            this.FullName = $"{returnType.FullName} {symbolName}({string.Join(',', parameterTypes.ToList().ConvertAll(x => x.FullName))})";
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
