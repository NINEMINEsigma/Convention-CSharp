using System.Text.RegularExpressions;

namespace Convention.RScript.Matcher
{
    public class GotoMatcher : IRSentenceMatcher
    {
        public bool Match(string expression, ref RScriptSentence sentence)
        {

            Regex GotoRegex = new(@"^goto\s*\(\s*(.+)\s*,\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*\)$");
            var GotoMatch = GotoRegex.Match(expression);
            if (GotoMatch.Success)
            {
                sentence.mode = RScriptSentence.Mode.Goto;
                sentence.content = GotoMatch.Groups[2].Value;
                sentence.info = new() { GotoMatch.Groups[1].Value, GotoMatch.Groups[2].Value };
                return true;
            }
            return false;
        }
    }
}
