using System.Text.RegularExpressions;

namespace Convention.RScript.Matcher
{
    public class BreakMatcher : IRSentenceMatcher
    {
        public bool Match(string expression, ref RScriptSentence sentence)
        {
            Regex LabelRegex = new(@"break\s*\(\s*(.+)\s*\)");
            var LabelMatch = LabelRegex.Match(expression);
            if (LabelMatch.Success)
            {
                sentence.mode = RScriptSentence.Mode.Breakpoint;
                sentence.content = LabelMatch.Groups[1].Value;
                return true;
            }
            return false;
        }
    }
}
