using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convention.Symbolization.Internal
{
    public class SymbolizationReader
    {
        #region Read Script Words

        private string buffer = "";
        private int lineContext = 1;
        private bool isOutOfStringline = true;
        public Dictionary<int, Dictionary<int, Variable>> ScriptWords = new() { { 1, new() } };

        private static HashSet<char> ControllerCharSet = new()
        {
            '{','}','(',')','[',']',
            '+','-','*','/','%',
            '=',
            '!','<','>','&','|',
            '^',
            ':',',','.','?',/*'\'','"',*/
            '@','#','$',
            ';'
        };

        public void ReadChar(char ch)
        {
            if (isOutOfStringline)
            {
                if (ControllerCharSet.Contains(ch))
                    CompleteSingleSymbol(ch);
                else if (ch == '\n')
                    CompleteLine();
                else if (char.IsWhiteSpace(ch) || ch == '\r' || ch == '\t')
                    CompleteWord();
                else if (buffer.Length == 0 && ch == '"')
                    BeginString();
                else if (char.IsLetter(ch) || char.IsDigit(ch))
                    PushChar(ch);
                else
                    throw new NotImplementedException($"{lineContext + 1} line, {buffer} + <{ch}> not implemented");
            }
            else
            {
                if ((buffer.Length > 0 && buffer.Last() == '\\') || ch != '"')
                    PushChar(ch);
                else if (ch == '"')
                    EndString();
                else
                    throw new NotImplementedException($"{lineContext + 1} line, \"{buffer}\" + \'{ch}\' not implemented");
            }
        }

        void CompleteSingleSymbol(char ch)
        {
            CompleteWord();
            buffer = ch.ToString();
            CompleteWord();
        }

        void PushChar(char ch)
        {
            buffer += ch;
        }
        void CompleteWord()
        {
            if (buffer.Length > 0)
            {
                if (ScriptWords.TryGetValue(lineContext, out var line) == false)
                {
                    ScriptWords.Add(lineContext, line = new());
                }
                if (Internal.Keyword.Keywords.TryGetValue(buffer, out var keyword))
                {
                    line.Add(line.Count + 1, keyword.Clone() as Internal.Variable);
                }
                else
                {
                    line.Add(line.Count + 1, new ScriptWordVariable(buffer));
                }
                buffer = "";
            }
        }
        void CompleteLine()
        {
            CompleteWord();
            lineContext++; ;
        }
        void BeginString()
        {
            if (buffer.Length > 0)
                throw new InvalidGrammarException($"String must start with nothing: {buffer}");
            isOutOfStringline = false;
        }
        void EndString()
        {
            isOutOfStringline = true;
            CompleteWord();
        }

        #endregion

        #region Read Scope Words

        private class KeywordEntry
        {
            public int line = 1;
            public int wordIndex = 1;
            public Dictionary<int, Dictionary<int, Variable>> Container = new();
            public KeywordEntry(int line, int wordIndex)
            {
                this.line = line;
                this.wordIndex = wordIndex;
            }
        }

        private Dictionary<Keyword, KeywordEntry> ScopeWords = new();

        public void CompleteScopeWord(SymbolizationContext rootContext)
        {
            Keyword currentKey = null;
            bool isNextKeyword = true;
            foreach(var line in ScriptWords)
            {
                int wordCounter = 1;
                foreach (var word in line.Value)
                {
                    if (isNextKeyword)
                    {
                        if (word.Value is Keyword cky)
                        {
                            currentKey = cky;
                            ScopeWords.Add(currentKey, new(line.Key, wordCounter));
                            isNextKeyword = false;
                        }
                        else
                        {
                            throw new InvalidGrammarException($"Line {line.Key}, word {wordCounter}: Expected a keyword, but got {word.Value}");
                        }
                    }
                    else
                    {
                        var container = ScopeWords[currentKey].Container;
                        if (container.ContainsKey(line.Key) == false)
                            container.Add(line.Key, new() { { wordCounter, word.Value } });
                        else
                            container[line.Key][wordCounter] = word.Value;
                        isNextKeyword = currentKey.ControlScope(rootContext, line.Key, word.Key, word.Value) == false;
                    }
                    wordCounter++;
                }
            }
        }

        #endregion
    }
}
