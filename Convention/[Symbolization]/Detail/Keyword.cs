using System;
using System.IO;
using System.Collections.Generic;

namespace Convention.Symbolization.Internal
{
    public abstract class Keyword : CloneableVariable<Keyword>
    {
        protected Keyword(string keyword) : base(keyword)
        {
            Keywords.TryAdd(keyword, this);
        }

        public static readonly Dictionary<string, Keyword> Keywords = new();

        public override bool Equals(Keyword other)
        {
            return this.GetType() == other.GetType();
        }

        public abstract Keyword ControlContext(SymbolizationContext context, ScriptWordVariable next);
    }

    public abstract class Keyword<T> : Keyword where T : Keyword<T>, new()
    {
        private static T MyInstance = new();

        protected Keyword(string keyword) : base(keyword)
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
    /// <b><see langword="import"/> file</b>
    /// </summary>
    public sealed class Import : Internal.Keyword<Import>
    {
        public Import() : base("import")
        {
        }

        public override Internal.Keyword CloneVariable(string targetSymbolName)
        {
            return new Import();
        }

        private ToolFile ImportFile = new("./");
        private string buffer = "";

        public override Internal.Keyword ControlContext(SymbolizationContext context, Internal.ScriptWordVariable next)
        {
            if (next.word == ";")
            {
                var importContext = new SymbolizationContext(context);
                importContext.Compile(ImportFile);
                return null;
            }
            else if(next.word==".")
            {
                ImportFile = ImportFile | buffer;
                buffer = "";
                if (ImportFile.Exists() == false)
                    throw new FileNotFoundException($"File path not found: {ImportFile}", ImportFile);
            }
            else
            {
                buffer += next.word;
            }
            return this;
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
    /// <b><see langword="def"/> FunctionName(parameter-list) return-type { ... return return-type-instance; }</b>
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
