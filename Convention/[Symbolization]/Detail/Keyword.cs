using System.Collections.Generic;
using System.IO;

namespace Convention.Symbolization.Internal
{
    public abstract class Keyword : CloneableVariable<Keyword>
    {
        protected Keyword(string keyword, int lineIndex, int wordIndex) : base(keyword, lineIndex, wordIndex) { }

        public static readonly Dictionary<string, Keyword> Keywords = new()
        {
            {  "import", new Convention.Symbolization.Keyword.Import(0,0) },
            { "namespace", new Convention.Symbolization.Keyword.Namespace(0,0) },
            { "def", new Convention.Symbolization.Keyword.FunctionDef(0,0) },
            { "return", new Convention.Symbolization.Keyword.Return(0,0) },
            { "if", new Convention.Symbolization.Keyword.If(0,0) },
            { "elif", new Convention.Symbolization.Keyword.Elif(0,0) },
            { "else", new Convention.Symbolization.Keyword.Else(0,0) },
            { "while", new Convention.Symbolization.Keyword.While(0,0) },
            { "break", new Convention.Symbolization.Keyword.Break(0,0) },
            { "continue", new Convention.Symbolization.Keyword.Continue(0,0) },
            { "struct", new Convention.Symbolization.Keyword.Structure(0,0) }
        };

        public override bool Equals(Keyword other)
        {
            return this.GetType() == other.GetType();
        }

        public abstract bool ControlScope(Variable next);

        public abstract void BuildScope(SymbolizationContext context, List<Variable> scopeWords);
    }

    public abstract class Keyword<T> : Keyword where T : Keyword<T>
    {
        protected Keyword(string keyword, int lineIndex, int wordIndex) : base(keyword, lineIndex, wordIndex) { }

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
        protected SingleKeyword(string keyword, int lineIndex, int wordIndex) : base(keyword, lineIndex, wordIndex)
        {
        }
        public override bool ControlScope(Internal.Variable next)
        {
            if (next is Internal.ScriptWordVariable swv && swv.Word == ";")
                return true;
            throw new InvalidGrammarException($"Expected ';' but got {next}");
        }

        public abstract void BuildScope();

        public sealed override void BuildScope(SymbolizationContext context, List<Internal.Variable> scopeWords)
        {
            BuildScope();
        }
    }

    public abstract class SentenceKeyword<T> : Internal.Keyword<T> where T : SentenceKeyword<T>
    {
        protected SentenceKeyword(string keyword, int lineIndex, int wordIndex) : base(keyword, lineIndex, wordIndex)
        {
        }
        public override bool ControlScope(Internal.Variable next)
        {
            if (next is not Internal.ScriptWordVariable swv)
                throw new InvalidGrammarException($"Not expected a key Word: {next}");
            return swv.Word != ";";
        }

        public abstract void BuildSentence(SymbolizationContext context, List<Internal.Variable> sentence);

        public sealed override void BuildScope(SymbolizationContext context, List<Internal.Variable> scopeWords)
        {
            BuildSentence(context, scopeWords.GetRange(0, scopeWords.Count - 1));
        }
    }

    public abstract class NamespaceKeyword<T> : Internal.Keyword<T> where T : NamespaceKeyword<T>
    {
        protected NamespaceKeyword(string keyword, int lineIndex, int wordIndex) : base(keyword, lineIndex, wordIndex)
        {
        }
        private enum Pause
        {
            Name, Body
        }

        private Pause pause = Pause.Name;
        private int layer = 0;

        public override bool ControlScope(Internal.Variable next)
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

        public abstract void BuildSpace(
            SymbolizationContext context,
            List<Internal.ScriptWordVariable> spaceExpression,
            Dictionary<Internal.Variable, Internal.Variable> scopeStartAndEnd,
            List<Internal.Variable> insideScopeWords
            );

        public override void BuildScope(SymbolizationContext context, List<Internal.Variable> scopeWords)
        {
            List<Internal.ScriptWordVariable> spaceExpression = new();
            Dictionary<Internal.Variable, Internal.Variable> scopeStartAndEnd = new();
            Stack<Internal.Variable> stack = new();
            bool IsSpaceExpression = true;
            foreach (var word in scopeWords)
            {
                var swv = word as Internal.ScriptWordVariable;
                if (IsSpaceExpression)
                {
                    if (swv.Word == "{")
                    {
                        IsSpaceExpression = false;
                        stack.Push(swv);
                        layer++;
                    }
                    else
                    {
                        spaceExpression.Add(swv);
                    }
                }
                else
                {
                    if (swv.Word == "{")
                    {
                        stack.Push(swv);
                        layer++;
                    }
                    else if (swv.Word == "}")
                    {
                        scopeStartAndEnd.Add(stack.Pop(), swv);
                        layer--;
                    }
                }
            }
            BuildSpace(context, spaceExpression, scopeStartAndEnd, 
                scopeWords.GetRange(spaceExpression.Count, scopeWords.Count - spaceExpression.Count));
        }
    }

    public abstract class VerbObjectScopeKeyword<T> : Internal.Keyword<T> where T : VerbObjectScopeKeyword<T>
    {
        protected VerbObjectScopeKeyword(string keyword, int lineIndex, int wordIndex) : base(keyword, lineIndex, wordIndex)
        {
        }

        private enum Pause
        {
            BeforeExpression, Expression, BeforeBody, Body, SingleSentence
        }

        private Pause pause = Pause.BeforeExpression;
        private int layer = 0;

        public override bool ControlScope(Internal.Variable next)
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
                if (next is Internal.ScriptWordVariable swv && swv.Word == "{")
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

        public abstract void BuildSpace(
            SymbolizationContext context,
            List<Internal.Variable> spaceExpression,
            Dictionary<Internal.Variable, Internal.Variable> scopeStartAndEnd,
            List<Internal.Variable> insideScopeWords
            );

        public override void BuildScope(SymbolizationContext context, List<Internal.Variable> scopeWords)
        {
            var pause = Pause.BeforeExpression; 
            List<Internal.Variable> expression = new();
            Dictionary<Internal.Variable, Internal.Variable> scopeStartAndEnd = new();
            List<Internal.Variable> insideScopeWords = new();
            Stack<Internal.Variable> stack = new();
            foreach (var word in scopeWords)
            {
                if (pause == Pause.BeforeExpression)
                {
                    pause = Pause.Expression;
                }
                else if (pause == Pause.Expression)
                {
                    if (word is Internal.ScriptWordVariable swv && swv.Word == ")")
                    {
                        pause = Pause.BeforeBody;
                    }
                    else
                    {
                        expression.Add(word);
                    }
                }
                else if (pause == Pause.BeforeBody)
                {
                    if (word is Internal.ScriptWordVariable swv && swv.Word == "{")
                    {
                        pause = Pause.Body;
                        stack.Push(word);
                    }
                    else
                    {
                        pause = Pause.SingleSentence;
                        insideScopeWords.Add(word);
                    }
                }
                else if (pause == Pause.Body)
                {
                    if (word is Internal.ScriptWordVariable swv)
                    {
                        if (swv.Word == "{")
                            stack.Push(word);
                        else if (swv.Word == "}")
                            scopeStartAndEnd.Add(stack.Pop(), word);
                    }
                    else
                        insideScopeWords.Add(word);
                }
                else if (pause == Pause.SingleSentence)
                {
                    insideScopeWords.Add(word);
                }
            }
            BuildSpace(context, expression, scopeStartAndEnd, insideScopeWords);
        }
    }

    /// <summary>
    /// <b><see langword="import"/> file</b>
    /// </summary>
    public sealed class Import : SentenceKeyword<Import>
    {
        public Import(int lineIndex, int wordIndex) : base("import", lineIndex, wordIndex)
        {
        }

        public override void BuildSentence(SymbolizationContext context, List<Internal.Variable> sentence)
        {
            ToolFile importPath = new(string.Join('/', sentence.ConvertAll(x => (x as Internal.ScriptWordVariable).Word)));
            if(importPath.Exists()==false)
                throw new FileNotFoundException(importPath.ToString());
            new SymbolizationContext(context).Compile(importPath);
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
        public Namespace(int lineIndex, int wordIndex) : base("namespace", lineIndex, wordIndex)
        {
        }

        public override void BuildSpace(
            SymbolizationContext context,
            List<Internal.ScriptWordVariable> spaceExpression,
            Dictionary<Internal.Variable, Internal.Variable> scopeStartAndEnd,
            List<Internal.Variable> insideScopeWords
            )
        {
            //TODO
            //将命名空间放在新的子上下文中编译
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
        public FunctionDef(int lineIndex, int wordIndex) : base("def", lineIndex, wordIndex)
        {
        }

        public override void BuildSpace(
            SymbolizationContext context, 
            List<Internal.ScriptWordVariable> spaceExpression, 
            Dictionary<Internal.Variable, Internal.Variable> scopeStartAndEnd,
            List<Internal.Variable> insideScopeWords
            )
        {
            //TODO
            //套用语法规则实现表达式向可执行语句的转换
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
        public Return(int lineIndex, int wordIndex) : base("return", lineIndex, wordIndex)
        {
        }

        public override void BuildSentence(SymbolizationContext context, List<Internal.Variable> sentence)
        {
            var symbol = sentence[0];
            //TODO
            //直接转换为可执行语句
            //在上下文中搜索被返回的symbol, 存在则返回对应对象, 不存在则返回symbol对象
            //运行时检查返回值是否满足函数签名的返回值, 否则抛出异常
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
        public If(int lineIndex, int wordIndex) : base("if", lineIndex, wordIndex)
        {
        }

        public override void BuildSpace(
            SymbolizationContext context, 
            List<Internal.Variable> spaceExpression, 
            Dictionary<Internal.Variable, Internal.Variable> scopeStartAndEnd,
            List<Internal.Variable> insideScopeWords
            )
        {
            //TODO
            //检查表达式是否成功, 成功则套用函数翻译规则将作用域翻译为可执行语句
            //否则跳转到末尾
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
        public Elif(int lineIndex, int wordIndex) : base("elif", lineIndex, wordIndex)
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
        public Else(int lineIndex, int wordIndex) : base("else", lineIndex, wordIndex)
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
        public While(int lineIndex, int wordIndex) : base("while", lineIndex, wordIndex)
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
        public Break(int lineIndex, int wordIndex) : base("break", lineIndex, wordIndex)
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
        public Continue(int lineIndex, int wordIndex) : base("continue", lineIndex, wordIndex)
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
        public Structure(int lineIndex, int wordIndex) : base("struct", lineIndex, wordIndex)
        {
        }
        public override Internal.Keyword CloneVariable(string targetSymbolName, int lineIndex, int wordIndex)
        {
            return new Structure(lineIndex, wordIndex);
        }
    }
}
