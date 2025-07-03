namespace Convention.Symbolization.Internal
{
    public class ScriptWordVariable : CloneableVariable<ScriptWordVariable>
    {
        public readonly string word;

        public ScriptWordVariable(string word) : base("")
        {
            this.word = word;
        }

        public override ScriptWordVariable CloneVariable(string targetSymbolName)
        {
            return new ScriptWordVariable(word);
        }

        public override bool Equals(ScriptWordVariable other)
        {
            return other is not null && word.Equals(other.word);
        }

        public Keyword GetKeyword()
        {
            if(Keyword.Keywords.TryGetValue(word, out var keyword))
                return keyword;
            return null;
        }

        public Variable GetVariable(SymbolizationContext context)
        {
            if (context.Variables.TryGetValue(word, out var variable))
            {
                return variable;
            }
            return null;
        }
    }
}
