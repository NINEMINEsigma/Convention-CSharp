using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convention.Symbolization.Internal
{
    public class SymbolizationReader
    {
        private string buffer = "";
        private int lineContext = 0;
        private bool isOutOfStringline = true;
        public Dictionary<int, List<Variable>> ScriptWords = new() { { 0, new() } };

        private static HashSet<char> ControllerCharSet = new()
        {
            '{','}','(',')','[',']',
            '+','-','*','/','%',
            '=',
            '!','<','>','&','|',
            '^',
            ':',',','.','?',/*'\'','"',*/
            '@','#','$',
        };

        public void ReadChar(char ch)
        {
            if (isOutOfStringline)
            {
                if (ControllerCharSet.Contains(ch))
                    CompleteSingleSymbol(ch);
                else if (char.IsWhiteSpace(ch) || ch == '\n' || ch == '\r' || ch == '\t')
                    CompleteWord();
                else if (buffer.Length == 0 && ch == '"')
                    BeginString();
                else if (ch == ';')
                    CompleteLine();
                else if (char.IsLetter(ch) || char.IsDigit(ch))
                    PushChar(ch);
                else
                    throw new NotImplementedException($"{lineContext+1} line, {buffer} + <{ch}> not implemented");
            }
            else
            {
                if ((buffer.Length > 0 && buffer.Last() == '\\') || ch != '"')
                    PushChar(ch);
                else if (ch == '"')
                    EndString();
                else
                    throw new NotImplementedException($"{lineContext+1} line, \"{buffer}\" + \'{ch}\' not implemented");
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
                ScriptWords[lineContext].Add(new ScriptWordVariable(buffer));
                buffer = "";
            }
        }
        void CompleteLine()
        {
            CompleteWord();
            ScriptWords[lineContext].Add(new ScriptWordVariable(";"));
            lineContext++;
            ScriptWords.Add(lineContext, new());
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
    }
}
