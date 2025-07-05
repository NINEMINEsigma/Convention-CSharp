namespace Convention.Symbolization.Internal
{
    public class ScriptWordVariable : CloneableVariable<ScriptWordVariable>
    {
        public string Word => this.SymbolInfo.SymbolName;

        public ScriptWordVariable(string word, int lineIndex, int wordIndex) : base((string)word.Clone(), lineIndex, wordIndex)
        {
        }

        public override ScriptWordVariable CloneVariable(string targetSymbolName, int lineIndex, int wordIndex)
        {
            return new ScriptWordVariable(Word, lineIndex, wordIndex);
        }

        public override bool Equals(ScriptWordVariable other)
        {
            return other is not null && Word.Equals(other.Word);
        }

        public override string ToString()
        {
            return Word;
        }
    }
}
