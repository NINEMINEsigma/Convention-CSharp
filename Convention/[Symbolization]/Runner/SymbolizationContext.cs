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

        private void Compile(Internal.SymbolizationReader reader)
        {
            reader.CompleteScopeWord(this);
        }

        public void Compile(Dictionary<int, Dictionary<int, Internal.Variable>> scriptWords)
        {
            // Turn the script words into scope words
            Compile(new Internal.SymbolizationReader() { ScriptWords = scriptWords });
        }

        public void Compile(string allText)
        {
            // Create a new reader to parse the script words
            Internal.SymbolizationReader reader = new();
            foreach (char ch in allText)
                reader.ReadChar(ch);
            // Turn the script words into scope words
            Compile(reader);
        }

        public void Compile(ToolFile file)
        {
            if (file.Exists() == false)
                throw new FileNotFoundException($"File not found: {file}", file);
            var text = file.LoadAsText();
            this.Compile(text);
        }
    }
}
