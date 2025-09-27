using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Convention.RScript
{
    public partial class ScriptContent
    {
        public object RuntimeBindingTarget;

        public struct RuntimeContext
        {
            public int iterator;
            public bool isResetIterator;

            public static RuntimeContext Create()
            {
                return new RuntimeContext()
                {
                    iterator = 0,
                    isResetIterator = false
                };
            }

            public RuntimeContext Clone()
            {
                return new()
                {
                    iterator = iterator,
                    isResetIterator = isResetIterator
                };
            }
        }

        public class ScriptDataEntry
        {
            public string text;

            public string command;
            public string[] parameters;
            public int iterator;

            public Func<RuntimeContext, RuntimeContext> actor;
        }

        private List<ScriptDataEntry> Commands = new();

        // 预处理工具

        public static int LastIndexOfNextTerminalTail(string text, int i, string terminal)
        {
            return text.IndexOf(terminal, i) + terminal.Length - 1;
        }

        public static bool StartWithSpecialTerminal(string text,int i)
        {
            switch (text[i])
            {
                case '{':
                case '}':
                    return true;
                default:
                    return false;
            }
        }

        public const string SingleLineTerminal = "//";
        public const string MultiLineBeginTerminal = "/*";
        public const string MultiLineEndTerminal = "*/";
        public const string TextTerminal = "\"";

        public static bool StartWithSingleLineTerminal(string text, int i)
        {
            int length = SingleLineTerminal.Length;
            if (text.Length - i < length)
                return false;
            return string.Compare(text, i, SingleLineTerminal, 0, length) == 0;
        }

        public static bool StartWithMultiLineTerminal(string text, int i)
        {
            int length = MultiLineBeginTerminal.Length;
            if (text.Length - i < length)
                return false;
            return string.Compare(text, i, MultiLineBeginTerminal, 0, length) == 0;
        }

        public static bool StartWithTextTerminal(string text, int i)
        {
            int length = TextTerminal.Length;
            if (text.Length - i < length)
                return false;
            return string.Compare(text, i, TextTerminal, 0, length) == 0;
        }

        public static int LastIndexOfNextTextTerminalTail(string text, int i,out string buffer)
        {
            char terminal = TextTerminal[0];
            int __head = i + 1;
            buffer = "";
            for (int __tail = text.Length; __head < __tail && text[__head] != terminal;)
            {
                if (text[__head] == '\\')
                {
                    switch (text[__head+1])
                    {
                        case 'n':
                            {
                                buffer += '\n';
                            }
                            break;
                        case 't':
                            {
                                buffer += '\t';
                            }
                            break;
                        case 'r':
                            {
                                buffer += '\r';
                            }
                            break;
                        case '0':
                            {
                                buffer += '\0';
                            }
                            break;
                        default:
                            {
                                buffer += text[__head + 1];
                            }
                            break;
                    }
                    __head += 2;
                }
                else
                {
                    buffer += text[__head];
                    __head++;
                }
            }
            return __head;
        }

        // 文本预处理
        private void ScriptTextPreprocessing(string script)
        {
            string buffer = "";
            void PushBuffer()
            {
                if (string.IsNullOrEmpty(buffer) == false)
                {
                    Commands.Add(new()
                    {
                        text = buffer.Trim(),
                        iterator = Commands.Count
                    });
                }
                buffer = "";
            }
            Func<int, bool> loopPr = x => x < script.Length;
            for (int i = 0; loopPr(i); i++)
            {
                // 切断语句\
                if (script[i] == ';')
                {
                    PushBuffer();
                }
                // 读入特殊标记符
                else if (StartWithSpecialTerminal(script, i))
                {
                    PushBuffer();
                    buffer += script[i];
                    PushBuffer();
                }
                // 跳过单行注释
                else if (StartWithSingleLineTerminal(script, i))
                {
                    i = LastIndexOfNextTerminalTail(script, i, "\n");
                }
                // 跳过多行注释
                else if (StartWithMultiLineTerminal(script, i))
                {
                    i = LastIndexOfNextTerminalTail(script, i, MultiLineEndTerminal);
                }
                // 读入文本
                else if (StartWithTextTerminal(script, i))
                {
                    i = LastIndexOfNextTextTerminalTail(script, i, out var temp);
                    buffer += temp;
                }
                // 读入
                else if (loopPr(i))
                    buffer += script[i];
            }
            // 存在未完成的语句
            if (string.IsNullOrEmpty(buffer) == false)
                throw new ArgumentException("The script did not end with the correct terminator.");
        }

        // 指令预处理工具

        private Dictionary<int, ScriptDataEntry> CommandIndexs = new();
        private Dictionary<string, int> LabelIndexs = new();

        public const string GotoTerminal = "goto";
        public const string LabelTerminal = "label";
        public const string IfTerminal = "if";

        // 指令预处理
        private void CommandPreprocessing()
        {
            static GroupCollection Match(ScriptDataEntry entry, [StringSyntax(StringSyntaxAttribute.Regex)] string pattern)
            {
                Match match = Regex.Match(entry.text, pattern);
                if (!match.Success)
                    throw new ArgumentException($"Script: \"{entry.text}\"<command iterator: {entry.iterator}> is invalid statement");
                return match.Groups;
            }

            // 加入自由映射
            foreach (var entry in Commands)
            {
                CommandIndexs.Add(entry.iterator, entry);
            }
            // 匹配
            foreach (var entry in Commands)
            {
                if (
                    // goto label-name;
                    entry.text.StartsWith(GotoTerminal)||
                    // label label-name;
                    entry.text.StartsWith(LabelTerminal)
                    )
                {
                    var groups = Match(entry, @"^(\S*)\s+([^\s]+?)\s*;$");
                    entry.command = groups[1].Value;
                    entry.parameters = new string[] { groups[2].Value };
                    // touch label
                    if (entry.command == LabelTerminal)
                    {
                        LabelIndexs.Add(groups[2].Value, entry.iterator);
                    }
                }
                else if (
                    // if(expr)
                    entry.text.StartsWith(IfTerminal)
                    )
                {
                    var groups = Match(entry, @"^(\S*)\s*(.*?)$");
                    entry.command = groups[1].Value;
                    entry.parameters = new string[] { groups[2].Value };
                }
                else
                {
                    var groups = Match(entry, @"^(\w+)\s*\(\s*(.*?)\s*\)\s*;$");
                    entry.command = groups[1].Value;

                    static string[] ParseArguments(string argumentsString)
                    {
                        if (string.IsNullOrWhiteSpace(argumentsString))
                            return new string[0];

                        // 处理字符串字面量和普通参数的正则表达式
                        string argPattern = @"""(?:[^""\\]|\\.)*""|[^,]+";

                        return Regex.Matches(argumentsString, argPattern)
                                    .Cast<Match>()
                                    .Select(m => m.Value.Trim())
                                    .Where(arg => !string.IsNullOrEmpty(arg))
                                    .ToArray();
                    }

                    entry.parameters = ParseArguments(groups[2].Value);
                }
            }
        }

        // 指令转化

        private void LoopForTC2AC()
        {
            Dictionary<string, MethodInfo> cache = (from method in RuntimeBindingTarget.GetType().GetMethods()
                                                    select new KeyValuePair<string, MethodInfo>(method.Name, method)).ToDictionary();

            for (int index = 0, e = Commands.Count; index < e; index++)
            {
                var entry = Commands[index];
                // Keywords
                if (entry.command == GotoTerminal)
                {
                    entry.actor = context =>
                    {
                        var next = context.Clone();
                        next.iterator = LabelIndexs[entry.parameters[0]];
                        next.isResetIterator = true;
                        return next;
                    };
                }
                // Custom Binding
                else if(cache.TryGetValue(entry.command,out var methodInfo))
                {
                    entry.actor = context =>
                    {

                        return context.Clone();
                    };
                }
                else
                {

                }
            }
        }

        public void LoadScript(string script)
        {
            ScriptTextPreprocessing(script);
            CommandPreprocessing();
            LoopForTC2AC();
        }
    }
}
