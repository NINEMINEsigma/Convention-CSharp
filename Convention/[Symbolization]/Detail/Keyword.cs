using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convention.Symbolization.Internal
{
    public abstract class Keyword : Variable
    {
        protected Keyword(string keyword, Type realType) : base(new(keyword, realType))
        {
            Keywords.Add(keyword, this);
        }

        public static readonly Dictionary<string, Keyword> Keywords = new();

        public abstract SymbolizationContext Execute(SymbolizationContext context, Variable[] leftParameters, Variable[] rightParameters);
    }

    public abstract class Keyword<T> : Keyword where T:Keyword<T>,new()
    {
        private static T MyInstance = new();

        public static T Instance
        {
            get
            {
                return MyInstance;
            }
        }

        protected Keyword(string keyword) : base(keyword, typeof(T))
        {
        }

        public override bool Equals(Variable other)
        {
            return MyInstance == other;
        }
    }

}

namespace Convention.Symbolization.Keyword
{
    public sealed class Namespace : Internal.Keyword<Namespace>
    {
        public Namespace() : base("namespace")
        {
        }
        public override SymbolizationContext Execute(SymbolizationContext context, Internal.Variable[] leftParameters, Internal.Variable[] rightParameters)
        {
            if (leftParameters.Length != 0)
                throw new InvalidGrammarException($"{this}: has invalid subject");
            if (rightParameters.Length != 1)
                throw new InvalidGrammarException($"{this}: needs to have one and only one object");
            Internal.Namespace subNamespace = context.CurrentNamespace.CreateOrGetSubNamespace(rightParameters[0].ToString());
            subNamespace.BeginUpdate();
            SymbolizationContext subContext = new(context, subNamespace);
            return subContext;
        }
    }

    public sealed class Return : Internal.Keyword<Return>
    {
        public Return() : base("return")
        {
        }

        public override SymbolizationContext Execute(SymbolizationContext context, Internal.Variable[] leftParameters, Internal.Variable[] rightParameters)
        {
            if (leftParameters.Length != 0)
                throw new InvalidGrammarException($"{this}: has invalid subject");
            if (rightParameters.Length > 1)
                throw new InvalidGrammarException($"{this}: has invalid object");
            // Inject return command to context
            return context;
        }
    }

    public sealed class While : Internal.Keyword<While>
    {
        public While() : base("while")
        {
        }
        public override SymbolizationContext Execute(SymbolizationContext context, Internal.Variable[] leftParameters, Internal.Variable[] rightParameters)
        {
            if (leftParameters.Length != 0)
                throw new InvalidGrammarException($"{this}: needs to have one and only one subject");
            if (rightParameters.Length != 1)
                throw new InvalidGrammarException($"{this}: has invalid object");
            SymbolizationContext subContext
            return context;
        }
    }

    public sealed class Break : Internal.Keyword<Break>
    {
        public Break() : base("break")
        {
        }
        public override SymbolizationContext Execute(SymbolizationContext context, Internal.Variable[] leftParameters, Internal.Variable[] rightParameters)
        {
            if (leftParameters.Length != 0)
                throw new InvalidGrammarException($"{this}: has invalid subject");
            if (rightParameters.Length != 0)
                throw new InvalidGrammarException($"{this}: has invalid object");
            return context;
        }
    }

    public sealed class Continue : Internal.Keyword<Continue>
    {
        public Continue() : base("continue")
        {
        }
        public override SymbolizationContext Execute(SymbolizationContext context, Internal.Variable[] leftParameters, Internal.Variable[] rightParameters)
        {
            if (leftParameters.Length != 0)
                throw new InvalidGrammarException($"{this}: has invalid subject");
            if (rightParameters.Length != 0)
                throw new InvalidGrammarException($"{this}: has invalid object");
            return context;
        }
    }

    public sealed class Structure : Internal.Keyword<Structure>
    {
        public Structure() : base("struct")
        {
        }

        public override SymbolizationContext Execute(SymbolizationContext context, Internal.Variable[] leftParameters, Internal.Variable[] rightParameters)
        {
            if (leftParameters.Length != 0)
                throw new InvalidGrammarException($"{this}: has invalid subject");
            if (rightParameters.Length != 1)
                throw new InvalidGrammarException($"{this}: needs to have one and only one object");
            return context;
        }

        public sealed class Function : Internal.Keyword<Function>
        {
            public Function() : base("def")
            {
            }

            public override SymbolizationContext Execute(SymbolizationContext context, Internal.Variable[] leftParameters, Internal.Variable[] rightParameters)
            {
                if (leftParameters.Length != 0)
                    throw new InvalidGrammarException($"{this}: has invalid subject");

            }
        }
    }
}
