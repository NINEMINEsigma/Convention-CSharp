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
    /// <summary>
    /// <b><see langword="import"/> namespace-expression</b>
    /// </summary>
    public sealed class Import : Internal.Keyword<Import>
    {
        public Import() : base("import")
        {
        }
    }

    /// <summary>
    /// <b><see langword="namespace"/> name { ... }</b>
    /// </summary>
    public sealed class Namespace : Internal.Keyword<Namespace>
    {
        public Namespace() : base("namespace")
        {
        }
    }

    /// <summary>
    /// <b><see langword="def"/> FunctionName(parameter-list) -> return-type { ... return return-type-instance; }</b>
    /// </summary>
    public sealed class FunctionDef : Internal.Keyword<FunctionDef>
    {
        public FunctionDef() : base("def")
        {
        }
    }

    /// <summary>
    /// <b><see langword="return"/> symbol;</b>
    /// </summary>
    public sealed class Return : Internal.Keyword<Return>
    {
        public Return() : base("return")
        {
        }
    }

    /// <summary>
    /// <b><see langword="if"/>(bool-expression) expression</b>
    /// </summary>
    public sealed class If : Internal.Keyword<If>
    {
        public If() : base("if")
        {
        }
    }

    /// <summary>
    /// <b><see langword="if"/> expression <see langword="else"/> expression</b>
    /// </summary>
    public sealed class Else : Internal.Keyword<Else>
    {
        public Else() : base("else")
        {
        }
    }

    /// <summary>
    /// <b><see langword="while"/>(bool-expression) expression</b>
    /// </summary>
    public sealed class While : Internal.Keyword<While>
    {
        public While() : base("while")
        {
        }
    }

    /// <summary>
    /// <b><see langword="break"/>;</b>
    /// </summary>
    public sealed class Break : Internal.Keyword<Break>
    {
        public Break() : base("break")
        {
        }
    }

    /// <summary>
    /// <b><see langword="continue"/>;</b>
    /// </summary>
    public sealed class Continue : Internal.Keyword<Continue>
    {
        public Continue() : base("continue")
        {
        }
    }

    /// <summary>
    /// <b><see langword="struct"/> structureName { ... }</b>
    /// </summary>
    public sealed class Structure : Internal.Keyword<Structure>
    {
        public Structure() : base("struct")
        {
        }
    }
}
