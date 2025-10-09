using Convention.RScript.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convention.RScript
{
    public class RScriptEngine
    {
        private ExpressionParser parser;
        private RScriptContext context;

        public Dictionary<string, RScriptVariableEntry> Run(string script, RScriptImportClass import = null, RScriptVariables variables = null)
        {
            parser = new(new());
            string newScript = "";
            foreach (var item in script.Split('\n'))
            {
                var line = item.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;
                if (line.StartsWith("//"))
                    continue;
                if (line.StartsWith('#'))
                    continue;
                newScript += line + ";";  // 添加分号分隔符
            }

            // 按分号分割并过滤空语句
            var statements = newScript.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(s => s.Trim())
                                     .Where(s => !string.IsNullOrEmpty(s))
                                     .ToArray();

            context = new(statements, import, variables);
            foreach (var type in context.Import)
                parser.context.Imports.AddType(type);
            return context.Run(parser);
        }
    }
}
