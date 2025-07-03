namespace Convention.Symbolization.Internal
{
    public class ScriptWordVariable : CloneableVariable<ScriptWordVariable>
    {
        public readonly string word;

        public ScriptWordVariable(string word) : base("")
        {
            this.word = (string)word.Clone();
        }

        public override ScriptWordVariable CloneVariable(string targetSymbolName)
        {
            return new ScriptWordVariable(word);
        }

        public override bool Equals(ScriptWordVariable other)
        {
            return other is not null && word.Equals(other.word);
        }

        public override string ToString()
        {
            return word;
        }
    }
}
