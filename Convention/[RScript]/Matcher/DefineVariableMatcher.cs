using System.Text.RegularExpressions;

namespace Convention.RScript.Matcher
{
    public class DefineVariableMatcher : IRSentenceMatcher
    {
        public bool Match(string expression, ref RScriptSentence sentence)
        {
            Regex DefineVariableRegex = new(@"(string|int|double|float|bool|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)");
            var DefineVariableMatch = DefineVariableRegex.Match(expression);
            if (DefineVariableMatch.Success)
            {
                sentence.mode = RScriptSentence.Mode.DefineVariable;
                sentence.info = new() { DefineVariableMatch.Groups[1].Value, DefineVariableMatch.Groups[2].Value };
                return true;
            }
            return false;
        }
    }
}
