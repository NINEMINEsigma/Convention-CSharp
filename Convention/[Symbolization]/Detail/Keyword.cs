using System;
using System.IO;
using System.Collections.Generic;

namespace Convention.Symbolization.Internal
{
    public abstract class Keyword : CloneableVariable<Keyword>
    {
        protected Keyword(string keyword, int lineIndex, int wordIndex) : base(keyword, lineIndex, wordIndex) { }

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

    public abstract class Keyword<T> : Keyword where T : Keyword<T>
    {
        protected Keyword(string keyword,int lineIndex,int wordIndex) : base(keyword, lineIndex, wordIndex) { }

        public override bool Equals(Variable other)
        {
            return other is T;
        }
    }
}

namespace Convention.Symbolization.Keyword
{
    public abstract class SingleKeyword<T> : Internal.Keyword<T> where T : SingleKeyword<T>
    {
        protected SingleKeyword(string keyword,int lineIndex,int wordIndex) : base(keyword, lineIndex, wordIndex)
        {
        }
        protected override bool ControlScope(SymbolizationContext context, Internal.Variable next)
        {
            if (next is not Internal.ScriptWordVariable swv ||swv.Word!=";")
                throw new InvalidGrammarException($"Expected ';' but got {next}");
            return true;
        }
    }

    public abstract class SentenceKeyword<T> : Internal.Keyword<T> where T : SentenceKeyword<T>
    {
        protected SentenceKeyword(string keyword,int lineIndex,int wordIndex) : base(keyword, lineIndex, wordIndex)
        {
        }
        protected override bool ControlScope(SymbolizationContext context, Internal.Variable next)
        {
            if (next is not Internal.ScriptWordVariable swv)
                throw new InvalidGrammarException($"Not expected a key Word: {next}");
            return swv.Word != ";";
        }
    }

    public abstract class NamespaceKeyword<T> : Internal.Keyword<T> where T : NamespaceKeyword<T>
    {
        protected NamespaceKeyword(string keyword,int lineIndex,int wordIndex) : base(keyword, lineIndex, wordIndex)
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
                    if (swv.Word == "{")
                    {
                        pause = Pause.Body;
                        layer++;
                    }
                    return true;
                }
                else
                {
                    throw new InvalidGrammarException($"Expected a script Word variable for namespace name, but got {next}");
                }
            }
            else
            {
                if (next is Internal.ScriptWordVariable swv)
                {
                    if (swv.Word == "{")
                        layer++;
                    else if (swv.Word == "}")
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

    public abstract class VerbObjectScopeKeyword<T> : Internal.Keyword<T> where T : VerbObjectScopeKeyword<T>
    {
        protected VerbObjectScopeKeyword(string keyword,int lineIndex,int wordIndex) : base(keyword, lineIndex, wordIndex)
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
                    throw new InvalidGrammarException($"Not expected a key Word: {next}");
                if (swv.Word == "(")
                    pause = Pause.Expression;
                else
                    throw new InvalidGrammarException($"Expected '(' symbol for expression start, but got {next}");
            }
            else if (pause == Pause.Expression)
            {
                if (next is Internal.ScriptWordVariable swv && swv.Word == ")")
                {
                    pause = Pause.BeforeBody;
                }
            }
            else if (pause == Pause.BeforeBody)
            {
                if (next is not Internal.ScriptWordVariable swv)
                    throw new InvalidGrammarException($"Not expected keyword for body start, but got {next}");
                if (swv.Word == "{")
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
                    if (swv.Word == "{")
                        layer++;
                    else if (swv.Word == "}")
                        layer--;
                    if (layer == 0)
                        return false;
                }
            }
            else if (pause == Pause.SingleSentence)
            {
                if (next is Internal.ScriptWordVariable swv && swv.Word == ";")
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
        public Import(int lineIndex,int wordIndex) : base("import",lineIndex,wordIndex)
        {
        }

        public override Internal.Keyword CloneVariable(string targetSymbolName, int lineIndex, int wordIndex)
        {
            return new Import(lineIndex, wordIndex);
        }
    }

    /// <summary>
    /// <b><see langword="namespace"/> name { ... }</b>
    /// </summary>
    public sealed class Namespace : NamespaceKeyword<Namespace>
    {
        public Namespace(int lineIndex,int wordIndex) : base("namespace", lineIndex, wordIndex)
        {
        }
        public override Internal.Keyword CloneVariable(string targetSymbolName, int lineIndex, int wordIndex)
        {
            return new Namespace(lineIndex, wordIndex);
        }
    }

    /// <summary>
    /// <b><see langword="def"/> FunctionName(parameter-list) return-type { ... return return-type-instance; }</b>
    /// </summary>
    public sealed class FunctionDef : NamespaceKeyword<FunctionDef>
    {
        public FunctionDef(int lineIndex,int wordIndex) : base("def", lineIndex, wordIndex)
        {
        }
        public override Internal.Keyword CloneVariable(string targetSymbolName, int lineIndex, int wordIndex)
        {
            return new FunctionDef(lineIndex, wordIndex);
        }
    }

    /// <summary>
    /// <b><see langword="return"/> symbol;</b>
    /// </summary>
    public sealed class Return : SentenceKeyword<Return>
    {
        public Return(int lineIndex,int wordIndex) : base("return", lineIndex, wordIndex)
        {
        }
        public override Internal.Keyword CloneVariable(string targetSymbolName, int lineIndex, int wordIndex)
        {
            return new Return(lineIndex, wordIndex);
        }
    }

    /// <summary>
    /// <b><see langword="if"/>(bool-expression) expression</b>
    /// </summary>
    public sealed class If : VerbObjectScopeKeyword<If>
    {
        public If(int lineIndex,int wordIndex) : base("if", lineIndex, wordIndex)
        {
        }
        public override Internal.Keyword CloneVariable(string targetSymbolName, int lineIndex, int wordIndex)
        {
            return new If(lineIndex, wordIndex);
        }
    }

    /// <summary>
    /// <b><see langword="if"/> expression <see langword="elif"/> expression</b><para></para>
    /// <b><see langword="if"/> expression <see langword="elif"/> expression <see langword="elif"/> expression...</b>
    /// </summary>
    public sealed class Elif : VerbObjectScopeKeyword<Elif>
    {
        public Elif(int lineIndex,int wordIndex) : base("elif", lineIndex, wordIndex)
        {
        }
        public override Internal.Keyword CloneVariable(string targetSymbolName, int lineIndex, int wordIndex)
        {
            return new Elif(lineIndex, wordIndex);
        }
    }

    /// <summary>
    /// <b><see langword="if"/> expression <see langword="else"/> expression</b><para></para>
    /// <b><see langword="if"/> expression <see langword="elif"/> expression... <see langword="else"/> expression</b>
    /// </summary>
    public sealed class Else : NamespaceKeyword<Else>
    {
        public Else(int lineIndex,int wordIndex) : base("else", lineIndex, wordIndex)
        {
        }
        public override Internal.Keyword CloneVariable(string targetSymbolName, int lineIndex, int wordIndex)
        {
            return new Else(lineIndex, wordIndex);
        }
    }

    /// <summary>
    /// <b><see langword="while"/>(bool-expression) expression</b>
    /// </summary>
    public sealed class While : VerbObjectScopeKeyword<While>
    {
        public While(int lineIndex,int wordIndex) : base("while", lineIndex, wordIndex)
        {
        }
        public override Internal.Keyword CloneVariable(string targetSymbolName, int lineIndex, int wordIndex)
        {
            return new While(lineIndex, wordIndex);
        }
    }

    /// <summary>
    /// <b><see langword="break"/>;</b>
    /// </summary>
    public sealed class Break : SingleKeyword<Break>
    {
        public Break(int lineIndex,int wordIndex) : base("break", lineIndex, wordIndex)
        {
        }
        public override Internal.Keyword CloneVariable(string targetSymbolName, int lineIndex, int wordIndex)
        {
            return new Break(lineIndex, wordIndex);
        }
    }

    /// <summary>
    /// <b><see langword="continue"/>;</b>
    /// </summary>
    public sealed class Continue : SingleKeyword<Continue>
    {
        public Continue(int lineIndex,int wordIndex) : base("continue", lineIndex, wordIndex)
        {
        }
        public override Internal.Keyword CloneVariable(string targetSymbolName, int lineIndex, int wordIndex)
        {
            return new Continue(lineIndex, wordIndex);
        }
    }

    /// <summary>
    /// <b><see langword="struct"/> structureName { ... }</b>
    /// </summary>
    public sealed class Structure : NamespaceKeyword<Structure>
    {
        public Structure(int lineIndex,int wordIndex) : base("struct", lineIndex, wordIndex)
        {
        }
        public override Internal.Keyword CloneVariable(string targetSymbolName, int lineIndex, int wordIndex)
        {
            return new Structure(lineIndex, wordIndex);
        }
    }
}
