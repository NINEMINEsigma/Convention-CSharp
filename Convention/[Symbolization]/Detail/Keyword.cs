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

        public virtual Keyword ControlContext(SymbolizationContext context, int line, int wordIndex, Variable next)
        {
            return ControlContext(context, next);
        }
        protected virtual Keyword ControlContext(SymbolizationContext context, Variable next)
        {
            return null;
        }
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

        protected override Internal.Keyword ControlContext(SymbolizationContext context, Internal.Variable next)
        {
            if (next is not Internal.ScriptWordVariable swv)
            {
                throw new InvalidGrammarException($"Not expected a key word: {next}");
            }
            if (swv.word == ";")
            {
                var importContext = new SymbolizationContext(context);
                importContext.Compile(ImportFile);
                return null;
            }
            else if (swv.word == ".")
            {
                ImportFile = ImportFile | buffer;
                buffer = "";
                if (ImportFile.Exists() == false)
                    throw new FileNotFoundException($"File path not found: {ImportFile}", ImportFile);
            }
            else
            {
                buffer += swv.word;
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

        public override Internal.Keyword CloneVariable(string targetSymbolName)
        {
            return new Namespace();
        }

        private enum Pause
        {
            Name, Body
        }

        private Pause pause = Pause.Name;
        private int layer = 0;
        private SymbolizationContext bulidingContext = null;
        private Dictionary<int, Dictionary<int, Internal.Variable>> subScriptWords = new();

        public override Internal.Keyword ControlContext(SymbolizationContext context,int line,int wordIndex, Internal.Variable next)
        {
            if (next is not Internal.ScriptWordVariable swv)
            {
                throw new InvalidGrammarException($"Not expected a key word: {next}");
            }
            if (pause == Pause.Name)
            {
                bulidingContext = new(context, context.CurrentNamespace.CreateOrGetSubNamespace(swv.word));
                bulidingContext.CurrentNamespace.BeginUpdate();
                pause = Pause.Body;
            }
            else
            {
                if (swv.word == "{")
                    layer++;
                else if (swv.word == "}")
                    layer--;
                if (layer == 0)
                {
                    bulidingContext.Compile(subScriptWords);
                    bulidingContext.CurrentNamespace.EndAndTryApplyUpdate();
                    return null;
                }
                else
                {
                    if (subScriptWords.TryGetValue(line, out var rline) == false)
                    {
                        subScriptWords.Add(line, rline = new());
                    }
                    rline.Add(wordIndex, next);
                }
            }
            return this;
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
