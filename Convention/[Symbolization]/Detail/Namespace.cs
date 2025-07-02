using System;
using System.Collections.Generic;
using System.Linq;

namespace Convention.Symbolization.Internal
{
    public sealed class Namespace : Variable<Namespace>, ICanFindVariable
    {
        private int Updateable = 0;

        public void BeginUpdate()
        {
            Updateable++;
        }

        private readonly Dictionary<string, Namespace> SubNamespaces = new();
        private readonly Dictionary<FunctionSymbol, Function> Functions = new();
        private readonly Dictionary<string, Structure> Structures = new();

        public Namespace CreateOrGetSubNamespace(string subNamespace)
        {
            if (Updateable == 0)
                throw new InvalidOperationException("Cannot create sub-namespace outside of an update block.");
            if(!SubNamespaces.TryGetValue(subNamespace,out var result))
            {
                result = new Namespace(subNamespace);
                SubNamespaces[subNamespace] = result;
            }
            return result;
        }

        public void AddFunction(Function function)
        {
            if (Updateable == 0)
                throw new InvalidOperationException("Cannot add function outside of an update block.");
            ArgumentNullException.ThrowIfNull(function);
            Functions.Add(function.FunctionInfo, function);
        }

        public void AddStructure(Structure structure)
        {
            if (Updateable == 0)
                throw new InvalidOperationException("Cannot add structure outside of an update block.");
            ArgumentNullException.ThrowIfNull(structure);
            Structures.Add(structure.SymbolInfo.SymbolName, structure);
        }

        public void EndAndTApplyUpdate()
        {
            Updateable--;
            if (Updateable == 0)
                Refresh();
        }

        private readonly Dictionary<string, int> SymbolCounter = new();
        private readonly Dictionary<string, List<Variable>> Symbol2Variable = new();

        public void Refresh()
        {
            // Refresh Symbols
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
                foreach (var symbol in ns.Value.Symbol2Variable)
                {
                    if (Symbol2Variable.TryGetValue(symbol.Key, out List<Variable> value))
                        value.AddRange(symbol.Value);
                    else
                        Symbol2Variable[symbol.Key] = new(symbol.Value);
                }
            }
            foreach (var func in Functions)
            {
                {
                    if (SymbolCounter.TryGetValue(func.Key.FunctionName, out var value))
                        SymbolCounter[func.Key.FunctionName] = ++value;
                    else
                        SymbolCounter[func.Key.FunctionName] = 1;
                }
                {
                    if (Symbol2Variable.TryGetValue(func.Key.FunctionName, out var value))
                        value.Add(func.Value);
                    else
                        Symbol2Variable[func.Key.FunctionName] = new() { func.Value };
                }
                {
                    if (Symbol2Variable.TryGetValue(func.Key.FunctionName, out var value))
                        value.Add(func.Value);
                    else
                        Symbol2Variable[func.Key.FunctionName] = new() { func.Value };
                }
            }
            foreach (var @struct in Structures)
            {
                {
                    if (SymbolCounter.TryGetValue(@struct.Key, out int value))
                        SymbolCounter[@struct.Key] = ++value;
                    else
                        SymbolCounter[@struct.Key] = 1;
                }
                {
                    if (Symbol2Variable.TryGetValue(@struct.Key, out List<Variable> value))
                        value.Add(@struct.Value);
                    else
                        Symbol2Variable[@struct.Key] = new() { @struct.Value };
                }
            }
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

        public Namespace FindSubNamespace(string subNamespaceName)
        {
            if (!SubNamespaces.TryGetValue(subNamespaceName, out var result))
                return null;
            return result;
        }

        public Function[] FindFunction(string symbolName)
        {
            if (!Symbol2Variable.TryGetValue(symbolName, out var result))
                return Array.Empty<Function>();
            return result.OfType<Function>().ToArray();
        }

        public Function FindFunctionInNamespace(FunctionSymbol symbol)
        {
            if (!Functions.TryGetValue(symbol, out var result))
                return null;
            return result;
        }

        public Structure[] FindStructure(string symbolName)
        {
            if (!Symbol2Variable.TryGetValue(symbolName, out var result))
                return Array.Empty<Structure>();
            return result.OfType<Structure>().ToArray();
        }

        public Structure FindStructureInNamespace(string symbolName)
        {
            if (!Structures.TryGetValue(symbolName, out var result))
                return null;
            return result;
        }
    }
}
