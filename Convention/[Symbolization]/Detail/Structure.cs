using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convention.Symbolization.Internal
{
    public sealed class Structure : Variable<Structure>, ICanFindVariable
    {
        private Dictionary<string, Variable> MemberFields;
        public readonly Namespace ParentNamespace;

        public Dictionary<string, Variable> CloneMemberFields()
        {
            return new(from pair in MemberFields
                       select new KeyValuePair<string, Variable>(
                           pair.Key,
                           pair.Value is ICloneable cloneable ? (cloneable.Clone() as Variable) : pair.Value)
                       );
        }

        private Structure(string symbolName, int lineIndex,int wordIndex, Namespace parentNamespace) : base(symbolName, lineIndex, wordIndex)
        {
            this.ParentNamespace = parentNamespace;
        }

        public static Structure Create(VariableSymbol symbol, Namespace parent)
        {
            Structure result = new(symbol.SymbolName, symbol.LineIndex, symbol.WordIndex, parent);
            parent.AddStructure(result);
            return result;
        }

        public override bool Equals(Structure other)
        {
            return this == other;
        }

        public Variable[] Find(string symbolName)
        {
            if (MemberFields.TryGetValue(symbolName, out var variable))
            {
                return new[] { variable };
            }
            return Array.Empty<Variable>();
        }
    }

    public sealed class StructureInstance : CloneableVariable<StructureInstance>,ICanFindVariable
    {
        public readonly Structure StructureType;
        private readonly Dictionary<string, Variable> MemberFields;
        public StructureInstance(string symbolName, int lineIndex, int wordIndex, Structure structureType)
            : base(symbolName, lineIndex, wordIndex)
        {
            this.StructureType = structureType;
            this.MemberFields = structureType.CloneMemberFields();
        }
        public override bool Equals(StructureInstance other)
        {
            return this == other;
        }
        public override StructureInstance CloneVariable(string targetSymbolName, int lineIndex, int wordIndex)
        {
            return new StructureInstance(targetSymbolName, lineIndex, wordIndex, this.StructureType);
        }

        public Variable GetField(string memberName)
        {
            if (MemberFields.TryGetValue(memberName, out var result))
                return result;
            else
                throw new KeyNotFoundException($"Member '{memberName}' not found in structure '{StructureType.SymbolInfo.SymbolName}'.");
        }

        public Variable[] Find(string symbolName)
        {
            if (MemberFields.TryGetValue(symbolName, out var variable))
            {
                return new[] { variable };
            }
            return Array.Empty<Variable>();
        }
    }
}

