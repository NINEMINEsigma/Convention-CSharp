using System.Text.RegularExpressions;

namespace Convention.RScript.Matcher
{
    public class BackMatcher : IRSentenceMatcher
    {
        public bool Match(string expression, ref RScriptSentence sentence)
        {
            Regex LabelRegex = new(@"back\s*\(\s*(.+)\s*\)");
            var LabelMatch = LabelRegex.Match(expression);
            if (LabelMatch.Success)
            {
                sentence.mode = RScriptSentence.Mode.Backpoint;
                sentence.content = LabelMatch.Groups[1].Value;
                return true;
            }
            return false;
        }
    }
}
