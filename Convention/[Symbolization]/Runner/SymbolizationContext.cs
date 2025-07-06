using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Convention.Symbolization
{
    public class SymbolizationContext
    {
        public SymbolizationContext() : this(null, Internal.Namespace.CreateRootNamespace()) { }
        public SymbolizationContext(SymbolizationContext parent) : this(parent, parent.CurrentNamespace) { }
        public SymbolizationContext(SymbolizationContext parent, Internal.Namespace newNamespace)
        {
            this.ParentContext = parent;
            this.CurrentNamespace = newNamespace;
        }

        private readonly SymbolizationContext ParentContext;
        public readonly Internal.Namespace CurrentNamespace;

        private void CompileToParent()
        {
            if (ParentContext == null)
                return;
        }

        // Compile begin at scope words to sub context
        public void Compile(Dictionary<Internal.Keyword, List<Internal.Variable>> scopeWords)
        {
            foreach (var (keyword, words) in scopeWords)
            {

            }
        }

        // Compile begin at script words to scope words
        private void CompileBeginAtScript2Scope(Internal.SymbolizationReader reader)
        {
            foreach (var word in reader.ScriptWords)
            {
                reader.ReadWord(word);
            }
            this.Compile(reader.ScopeWords);
        }

        // Compile begin at script words to scope words
        public void Compile(List<Internal.Variable> scriptWords)
        {
            this.CompileBeginAtScript2Scope(new() { ScriptWords = scriptWords });
        }

        // Compile begin at text to script words
        public void Compile(string allText)
        {
            Internal.SymbolizationReader reader = new();
            reader ??= new();
            foreach (char ch in allText)
                reader.ReadChar(ch);
            // Turn the script words into scope words
            this.CompileBeginAtScript2Scope(reader);
        }

        // Compile begin at text to script words
        public void Compile(ToolFile file)
        {
            if (file.Exists() == false)
                throw new FileNotFoundException($"File not found: {file}", file);
            var text = file.LoadAsText();
            this.Compile(text);
        }
    }
}
