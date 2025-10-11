using System.Text.RegularExpressions;

namespace Convention.RScript.Matcher
{
    public class LabelMatcher : IRSentenceMatcher
    {
        public bool Match(string expression, ref RScriptSentence sentence)
        {
            Regex LabelRegex = new(@"label\s*\(\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*\)");
            var LabelMatch = LabelRegex.Match(expression);
            if (LabelMatch.Success)
            {
                sentence.mode = RScriptSentence.Mode.Label;
                sentence.content = LabelMatch.Groups[1].Value;
                return true;
            }
            return false;
        }
    }
}
