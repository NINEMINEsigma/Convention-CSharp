using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convention.RScript
{
    public class ScriptRunner
    {
        public ScriptContent BuildNewContent(object target)
        {
            return new()
            {
                RuntimeBindingTarget = target,
            };
        }

        public ScriptContent BuildNewContent()
        {
            return BuildNewContent(null);
        }

        public void RunScriptFromContent(ScriptContent content)
        {

        }
    }
}
