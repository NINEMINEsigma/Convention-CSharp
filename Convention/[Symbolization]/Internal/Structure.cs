using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convention.Symbolization.Internal
{
    public sealed class Structure : Variable
    {
        public readonly string Name;
        private Dictionary<VariableSymbol, Variable> VariableSymbolAndDefaultValue;

        private Structure(string name)
        {
            this.Name = name;
        }
        public Structure(string name, Dictionary<VariableSymbol, Variable> variableSymbolAndDefaultValue)
        {
            this.Name = name;
            this.VariableSymbolAndDefaultValue = variableSymbolAndDefaultValue;
        }

        public override object Clone()
        {
            Structure target = new(Name);
            foreach (var pair in VariableSymbolAndDefaultValue)
            {
                target.VariableSymbolAndDefaultValue[pair.Key] = pair.Value;
            }
            return target;
        }

        public bool Equals(Structure other)
        {
            return Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
