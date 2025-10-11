namespace Convention.RScript.Matcher
{
    public class NamespaceMater : IRSentenceMatcher
    {
        public bool Match(string expression, ref RScriptSentence sentence)
        {
            if (expression == "{")
            {
                sentence.mode = RScriptSentence.Mode.EnterNamespace;
                return true;
            }
            else if (expression == "}")
            {
                sentence.mode = RScriptSentence.Mode.ExitNamespace;
                return true;
            }
            return false;
        }
    }
}
