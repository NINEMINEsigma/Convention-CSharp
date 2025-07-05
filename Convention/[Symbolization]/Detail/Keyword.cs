using System;
using System.IO;
using System.Collections.Generic;

namespace Convention.Symbolization.Internal
{
    public abstract class Keyword : CloneableVariable<Keyword>
    {
        protected Keyword(string keyword) : base(keyword) { }

        public static readonly Dictionary<string, Keyword> Keywords = new()
        {
            {  "import", new Convention.Symbolization.Keyword.Import() },
            { "namespace", new Convention.Symbolization.Keyword.Namespace() },
            { "def", new Convention.Symbolization.Keyword.FunctionDef() },
            { "return", new Convention.Symbolization.Keyword.Return() },
            { "if", new Convention.Symbolization.Keyword.If() },
            { "elif", new Convention.Symbolization.Keyword.Elif() },
            { "else", new Convention.Symbolization.Keyword.Else() },
            { "while", new Convention.Symbolization.Keyword.While() },
            { "break", new Convention.Symbolization.Keyword.Break() },
            { "continue", new Convention.Symbolization.Keyword.Continue() },
            { "struct", new Convention.Symbolization.Keyword.Structure() }
        };

        public override bool Equals(Keyword other)
        {
            return this.GetType() == other.GetType();
        }

        public virtual bool ControlScope(SymbolizationContext context, int line, int wordIndex, Variable next)
        {
            return ControlScope(context, next);
        }
        protected virtual bool ControlScope(SymbolizationContext context, Variable next)
        {
            return true;
        }
    }

    public abstract class Keyword<T> : Keyword where T : Keyword<T>, new()
    {
        protected Keyword(string keyword) : base(keyword) { }

        public override bool Equals(Variable other)
        {
            return other is T;
        }

        public override Keyword CloneVariable(string targetSymbolName)
        {
            return new T();
        }
    }
}

namespace Convention.Symbolization.Keyword
{
    public abstract class SingleKeyword<T> : Internal.Keyword<T> where T : SingleKeyword<T>, new()
    {
        protected SingleKeyword(string keyword) : base(keyword)
        {
        }
        protected override bool ControlScope(SymbolizationContext context, Internal.Variable next)
        {
            if (next is not Internal.ScriptWordVariable swv ||swv.word!=";")
                throw new InvalidGrammarException($"Expected ';' but got {next}");
            return true;
        }
    }

    public abstract class SentenceKeyword<T> : Internal.Keyword<T> where T : SentenceKeyword<T>, new()
    {
        protected SentenceKeyword(string keyword) : base(keyword)
        {
        }
        protected override bool ControlScope(SymbolizationContext context, Internal.Variable next)
        {
            if (next is not Internal.ScriptWordVariable swv)
                throw new InvalidGrammarException($"Not expected a key word: {next}");
            return swv.word != ";";
        }
    }

    public abstract class NamespaceKeyword<T> : Internal.Keyword<T> where T : NamespaceKeyword<T>, new()
    {
        protected NamespaceKeyword(string keyword) : base(keyword)
        {
        }
        private enum Pause
        {
            Name, Body
        }

        private Pause pause = Pause.Name;
        private int layer = 0;

        protected override bool ControlScope(SymbolizationContext context, Internal.Variable next)
        {
            if (pause == Pause.Name)
            {
                if (next is Internal.ScriptWordVariable swv)
                {
                    if (swv.word == "{")
                    {
                        pause = Pause.Body;
                        layer++;
                    }
                    return true;
                }
                else
                {
                    throw new InvalidGrammarException($"Expected a script word variable for namespace name, but got {next}");
                }
            }
            else
            {
                if (next is Internal.ScriptWordVariable swv)
                {
                    if (swv.word == "{")
                        layer++;
                    else if (swv.word == "}")
                        layer--;
                    if (layer == 0)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }

    public abstract class VerbObjectScopeKeyword<T> : Internal.Keyword<T> where T : VerbObjectScopeKeyword<T>, new()
    {
        protected VerbObjectScopeKeyword(string keyword) : base(keyword)
        {
        }

        private enum Pause
        {
            BeforeExpression, Expression, BeforeBody, Body, SingleSentence
        }

        private Pause pause = Pause.BeforeExpression;
        private int layer = 0;

        protected override bool ControlScope(SymbolizationContext context, Internal.Variable next)
        {
            if (pause == Pause.BeforeExpression)
            {
                if (next is not Internal.ScriptWordVariable swv)
                    throw new InvalidGrammarException($"Not expected a key word: {next}");
                if (swv.word == "(")
                    pause = Pause.Expression;
                else
                    throw new InvalidGrammarException($"Expected '(' symbol for expression start, but got {next}");
            }
            else if (pause == Pause.Expression)
            {
                if (next is Internal.ScriptWordVariable swv && swv.word == ")")
                {
                    pause = Pause.BeforeBody;
                }
            }
            else if (pause == Pause.BeforeBody)
            {
                if (next is not Internal.ScriptWordVariable swv)
                    throw new InvalidGrammarException($"Not expected keyword for body start, but got {next}");
                if (swv.word == "{")
                {
                    pause = Pause.Body;
                    layer++;
                }
                else
                    pause = Pause.SingleSentence;
            }
            else if (pause == Pause.Body)
            {
                if (next is Internal.ScriptWordVariable swv)
                {
                    if (swv.word == "{")
                        layer++;
                    else if (swv.word == "}")
                        layer--;
                    if (layer == 0)
                        return false;
                }
            }
            else if (pause == Pause.SingleSentence)
            {
                if (next is Internal.ScriptWordVariable swv && swv.word == ";")
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// <b><see langword="import"/> file</b>
    /// </summary>
    public sealed class Import : SentenceKeyword<Import>
    {
        public Import() : base("import")
        {
        }
    }

    /// <summary>
    /// <b><see langword="namespace"/> name { ... }</b>
    /// </summary>
    public sealed class Namespace : NamespaceKeyword<Namespace>
    {
        public Namespace() : base("namespace")
        {
        }
    }

    /// <summary>
    /// <b><see langword="def"/> FunctionName(parameter-list) return-type { ... return return-type-instance; }</b>
    /// </summary>
    public sealed class FunctionDef : NamespaceKeyword<FunctionDef>
    {
        public FunctionDef() : base("def")
        {
        }
    }

    /// <summary>
    /// <b><see langword="return"/> symbol;</b>
    /// </summary>
    public sealed class Return : SentenceKeyword<Return>
    {
        public Return() : base("return")
        {
        }
    }

    /// <summary>
    /// <b><see langword="if"/>(bool-expression) expression</b>
    /// </summary>
    public sealed class If : VerbObjectScopeKeyword<If>
    {
        public If() : base("if")
        {
        }
    }

    /// <summary>
    /// <b><see langword="if"/> expression <see langword="elif"/> expression</b><para></para>
    /// <b><see langword="if"/> expression <see langword="elif"/> expression <see langword="elif"/> expression...</b>
    /// </summary>
    public sealed class Elif : VerbObjectScopeKeyword<Elif>
    {
        public Elif() : base("elif")
        {
        }
    }

    /// <summary>
    /// <b><see langword="if"/> expression <see langword="else"/> expression</b><para></para>
    /// <b><see langword="if"/> expression <see langword="elif"/> expression... <see langword="else"/> expression</b>
    /// </summary>
    public sealed class Else : NamespaceKeyword<Else>
    {
        public Else() : base("else")
        {
        }
    }

    /// <summary>
    /// <b><see langword="while"/>(bool-expression) expression</b>
    /// </summary>
    public sealed class While : VerbObjectScopeKeyword<While>
    {
        public While() : base("while")
        {
        }
    }

    /// <summary>
    /// <b><see langword="break"/>;</b>
    /// </summary>
    public sealed class Break : SingleKeyword<Break>
    {
        public Break() : base("break")
        {
        }
    }

    /// <summary>
    /// <b><see langword="continue"/>;</b>
    /// </summary>
    public sealed class Continue : SingleKeyword<Continue>
    {
        public Continue() : base("continue")
        {
        }
    }

    /// <summary>
    /// <b><see langword="struct"/> structureName { ... }</b>
    /// </summary>
    public sealed class Structure : NamespaceKeyword<Structure>
    {
        public Structure() : base("struct")
        {
        }
    }
}
