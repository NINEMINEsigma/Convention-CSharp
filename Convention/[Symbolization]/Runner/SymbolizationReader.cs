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
        public Dictionary<int, List<Variable>> ScriptWords = new() { { 1, new() } };

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
                    line.Add(keyword.Clone() as Internal.Variable);
                }
                else
                {
                    line.Add(new ScriptWordVariable(buffer));
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

        public void CompleteScopeWord(SymbolizationContext rootContext)
        {
            int wordCounter = 1;
            Internal.Keyword currentKey = null;
            foreach(var line in ScriptWords)
            {
                foreach (var word in line.Value)
                {
                    if (currentKey == null)
                    {
                        if(currentKey is Internal.Keyword cky)
                        {
                            currentKey = cky;
                        }
                        else
                        {
                            throw new InvalidGrammarException($"Line {line.Key}, word {wordCounter}: Expected a keyword, but got {word}");
                        }
                    }
                    else
                    {
                        currentKey = currentKey.ControlContext(rootContext, word);
                    }
                    wordCounter++;
                }
            }
        }

        #endregion
    }
}
