using System;
using System.Collections.Generic;
using System.Linq;

namespace Convention.Symbolization.Internal
{
    public sealed class Namespace : Variable<Namespace>
    {
        public Dictionary<string, Namespace> SubNamespaces = new();
        public Dictionary<FunctionSymbol, Function> Functions = new();
        public Dictionary<string, Structure> Structures = new();

        private readonly Dictionary<string, int> SymbolCounter = new();
        private readonly Dictionary<string, List<Variable>> Symbol2Variable = new();

        public void Refresh()
        {
            // Refresh SymbolCounter
            SymbolCounter.Clear();
            foreach (var ns in SubNamespaces)
            {
                ns.Value.Refresh();
                foreach (var symbol in ns.Value.SymbolCounter)
                {
                    if (SymbolCounter.ContainsKey(symbol.Key))
                        SymbolCounter[symbol.Key] += symbol.Value;
                    else
                        SymbolCounter[symbol.Key] = symbol.Value;
                }
            }
            foreach (var func in Functions)
            {
                if (SymbolCounter.ContainsKey(func.Key.FunctionName))
                    SymbolCounter[func.Key.FunctionName]++;
                else
                    SymbolCounter[func.Key.FunctionName] = 1;
            }
            foreach (var @struct in Structures)
            {
                if (SymbolCounter.ContainsKey(@struct.Key))
                    SymbolCounter[@struct.Key]++;
                else
                    SymbolCounter[@struct.Key] = 1;
            }
        }

        public static Namespace CreateOrGetSubNamespace(string namespaceName, Namespace parent)
        {
            if (parent.SubNamespaces.TryGetValue(namespaceName, out var sub))
                return sub;
            parent.SubNamespaces[namespaceName] = new(namespaceName);
            return parent.SubNamespaces[namespaceName];
        }
        public static Namespace CreateRootNamespace()
        {
            return new("global");
        }

        private Namespace(string symbolName) : base(symbolName) { }

        public override bool Equals(Namespace other)
        {
            return this == other;
        }

        public string GetNamespaceName() => this.SymbolInfo.SymbolName;

        public bool ContainsSymbol(string symbolName)
        {
            return SymbolCounter.ContainsKey(symbolName);
        }

        public int CountSymbol(string symbolName)
        {
            return SymbolCounter.TryGetValue(symbolName, out var count) ? count : 0;
        }

        public Variable[] Find(string symbolName)
        {
            if (!Symbol2Variable.TryGetValue(symbolName,out var result))
                return Array.Empty<Variable>();
            return result.ToArray();
        }
    }
}
