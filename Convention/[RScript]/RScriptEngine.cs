using Convention.RScript.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Convention.RScript
{
    public class RScriptEngine
    {
        private ExpressionParser parser;
        private RScriptContext context;

        private IEnumerable<string> SplitScript(string script)
        {
            StringBuilder builder = new();
            List<string> statements = new();

            for (int i = 0, e = script.Length; i < e; i++)
            {
                char c = script[i];
                if (c == ';')
                {
                    if (builder.Length > 0)
                    {
                        statements.Add(builder.ToString().Trim());
                        builder.Clear();
                    }
                }
                else if (c == '/' && i + 1 < e)
                {
                    // Skip single-line comment
                    if (script[i + 1] == '/')
                    {
                        while (i < script.Length && script[i] != '\n')
                            i++;
                    }
                    // Skip multi-line comment
                    else if (script[i + 1] == '*')
                    {
                        i += 2;
                        while (i + 1 < script.Length && !(script[i] == '*' && script[i + 1] == '/'))
                            i++;
                        i++;
                    }
                    else
                    {
                        builder.Append(c);
                    }
                }
                else if (c == '#')
                {
                    // Skip single-line comment
                    while (i < script.Length && script[i] != '\n')
                        i++;
                }
                else if (c == '\"')
                {
                    for (i++; i < e; i++)
                    {
                        builder.Append(script[i]);
                        if (script[i] == '\"')
                        {
                            break;
                        }
                        else if (script[i] == '\\')
                        {
                            i++;
                            if (i < e)
                                builder.Append(script[i]);
                            else
                                throw new RScriptException("Invalid escape sequence in string literal", -1);
                        }
                    }
                }
                else if (c == '{' || c == '}')
                {
                    // Treat braces as statement separators
                    if (builder.Length > 0)
                    {
                        statements.Add(builder.ToString().Trim());
                        builder.Clear();
                    }
                    statements.Add(c.ToString());
                }
                else
                {
                    builder.Append(c);
                }
            }
            if (builder.Length > 0)
            {
                statements.Add(builder.ToString().Trim());
                builder.Clear();
            }

            return statements.Where(s => !string.IsNullOrWhiteSpace(s));
        }

        public Dictionary<string, RScriptVariableEntry> Run(string script, RScriptImportClass import = null, RScriptVariables variables = null)
        {
            parser = new(new());
            context = new(SplitScript(script).ToArray(), import, variables);
            foreach (var type in context.Import)
                parser.context.Imports.AddType(type);
            return context.Run(parser);
        }
    }
}
