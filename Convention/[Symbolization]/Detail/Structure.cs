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

        private Structure(string symbolName, Namespace parentNamespace) : base(symbolName)
        {
            this.ParentNamespace = parentNamespace;
        }

        public static Structure Create(string structureName, Namespace parent)
        {
            Structure result = new(structureName, parent);
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
        public StructureInstance(string symbolName, Structure structureType)
            : base(symbolName)
        {
            this.StructureType = structureType;
            this.MemberFields = structureType.CloneMemberFields();
        }
        public override bool Equals(StructureInstance other)
        {
            return this == other;
        }
        public override StructureInstance CloneVariable(string targetSymbolName)
        {
            return new StructureInstance(targetSymbolName, this.StructureType);
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

