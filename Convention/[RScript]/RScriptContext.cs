using Convention.RScript.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Convention.RScript
{
    public struct RScriptSentence
    {
        public enum Mode
        {
            /// <summary>
            /// 表达式, 格式: 任意合法表达式
            /// </summary>
            Expression,
            /// <summary>
            /// 定义变量, 格式: 类型 变量名
            /// <para>类型支持: string, int, double, float, bool, var</para>
            /// <para>每层命名空间中不可重复定义变量, 不可使用未定义的变量, 不存在时会自动向上查找上级空间的变量</para>
            /// </summary>
            DefineVariable,
            /// <summary>
            /// 进入新的命名空间, 格式: {
            /// <para>命名空间是一对花括号包裹内容空间, 格式: {...}</para>
            /// </summary>
            EnterNamespace,
            /// <summary>
            /// 退出当前命名空间, 格式: }
            /// <para>命名空间是一对花括号包裹内容空间, 格式: {...}</para>
            /// </summary>
            ExitNamespace,
            /// <summary>
            /// 标签, 格式: label(labelname)
            /// </summary>
            Label,
            /// <summary>
            /// 跳转到指定标签, 格式: goto(a,b,labelname)
            /// <para>当a大于b时跳转到labelname</para>
            /// </summary>
            Goto,
        }

        public string content;
        public List<string> info;
        public Mode mode;
    }

    public partial class RScriptContext
    {
        public readonly RScriptImportClass Import;
        public readonly RScriptVariables Variables;
        private readonly RScriptSentence[] Sentences;
        private readonly Dictionary<string, int> Labels;
        private readonly Dictionary<int, int> Namespace;

        private static RScriptSentence ParseToSentence(string expression)
        {
            RScriptSentence result = new()
            {
                content = expression,
                mode = RScriptSentence.Mode.Expression
            };
            expression = expression.Trim();
            expression.TrimEnd(';');
            if (expression == "{")
            {
                result.mode = RScriptSentence.Mode.EnterNamespace;
            }
            else if (expression == "}")
            {
                result.mode = RScriptSentence.Mode.ExitNamespace;
            }

            Regex DefineVariableRegex = new(@"^(string|int|double|float|bool|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)$");
            var DefineVariableMatch = DefineVariableRegex.Match(expression);
            if (DefineVariableMatch.Success)
            {
                result.mode = RScriptSentence.Mode.DefineVariable;
                result.info = new() { DefineVariableMatch.Groups[1].Value, DefineVariableMatch.Groups[2].Value };
                return result;
            }

            Regex LabelRegex = new(@"^label\s*\(\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*\)$");
            var LabelMatch = LabelRegex.Match(expression);
            if (LabelMatch.Success)
            {
                result.mode = RScriptSentence.Mode.Label;
                result.content = LabelMatch.Groups[1].Value;
                return result;
            }

            Regex GotoRegex = new(@"^goto\s*\(\s*(.+)\s*,\s*(.*)\s*,([a-zA-Z_][a-zA-Z0-9_]*)\s*\)$");
            var GotoMatch = GotoRegex.Match(expression);
            if (GotoMatch.Success)
            {
                result.mode = RScriptSentence.Mode.Goto;
                result.content = GotoMatch.Groups[3].Value;
                result.info = new() { GotoMatch.Groups[1].Value, GotoMatch.Groups[2].Value, GotoMatch.Groups[3].Value };
                return result;
            }

            return result;
        }
        private void BuildUpLabelsAndNamespace(ref Dictionary<string, int> labelIndicator, ref Dictionary<int, int> namespaceIndicator)
        {
            Stack<int> namespaceLayers = new();
            for (int i = 0, e = Sentences.Length; i != e; i++)
            {
                if (Sentences[i].mode == RScriptSentence.Mode.Label)
                {
                    labelIndicator[Sentences[i].content] = i;
                }
                else if (Sentences[i].mode == RScriptSentence.Mode.EnterNamespace)
                {
                    namespaceLayers.Push(i);
                }
                else if (Sentences[i].mode == RScriptSentence.Mode.ExitNamespace)
                {
                    if (namespaceLayers.Count == 0)
                        throw new RScriptException("Namespace exit without enter.", i);
                    var enterPointer = namespaceLayers.Pop();
                    namespaceIndicator[enterPointer] = i;
                }
            }
            if (namespaceLayers.Count > 0)
            {
                throw new RScriptException("Namespace enter without exit.", namespaceLayers.Peek());
            }
        }

        public RScriptContext(string[] expressions, RScriptImportClass import = null, RScriptVariables variables = null)
        {
            this.Import = import ?? new();
            this.Variables = variables ?? new();
            this.Sentences = (from item in expressions select ParseToSentence(item)).ToArray();
            this.Labels = new();
            this.Namespace = new();
            BuildUpLabelsAndNamespace(ref this.Labels, ref this.Namespace);
        }


        public RScriptSentence CurrentSentence => Sentences[CurrentRuntimePointer];

        private void RunNextStep(ExpressionParser parser)
        {
            var sentence = CurrentSentence;
            switch (sentence.mode)
            {
                case RScriptSentence.Mode.Expression:
                    {
                        // 执行表达式
                        parser.Evaluate(sentence.content);
                    }
                    break;
                case RScriptSentence.Mode.DefineVariable:
                    {
                        // 定义变量
                        var varTypeName = sentence.info[0];
                        var varName = sentence.info[1];
                        Type varType;
                        object varDefaultValue;
                        {
                            if (varTypeName == "string")
                            {
                                varType = typeof(string);
                                varDefaultValue = string.Empty;
                            }
                            else if (varTypeName == "int")
                            {
                                varType = typeof(int);
                                varDefaultValue = 0;
                            }
                            else if (varTypeName == "double")
                            {
                                varType = typeof(double);
                                varDefaultValue = 0.0;
                            }
                            else if (varTypeName == "float")
                            {
                                varType = typeof(float);
                                varDefaultValue = 0.0f;
                            }
                            else if (varTypeName == "bool")
                            {
                                varType = typeof(bool);
                                varDefaultValue = false;
                            }
                            else if (varTypeName == "var")
                            {
                                varType = typeof(object);
                                varDefaultValue = null;
                            }
                            else
                            {
                                throw new RScriptException($"Unsupported variable type '{varTypeName}'.", CurrentRuntimePointer);
                            }
                        }
                        if (CurrentLocalSpaceVariableNames.Peek().Contains(varName) == false)
                        {
                            Variables.Add(varName, new() { type = varType, data = varDefaultValue });
                            parser.context.Variables[varName] = varDefaultValue;
                            CurrentLocalSpaceVariableNames.Peek().Add(varName);
                        }
                        else
                        {
                            throw new RScriptException($"Variable '{varName}' already defined on this namespace.", CurrentRuntimePointer);
                        }
                    }
                    break;
                case RScriptSentence.Mode.EnterNamespace:
                    {
                        // 准备记录当前命名空间中定义的变量, 清空上层命名空间的变量
                        CurrentLocalSpaceVariableNames.Push(new());
                        // 更新变量值
                        foreach (var (varName, varValue) in parser.context.Variables)
                        {
                            Variables.SetValue(varName, varValue);
                        }
                    }
                    break;
                case RScriptSentence.Mode.ExitNamespace:
                    {
                        // 移除在本命名空间中定义的变量
                        foreach (var local in CurrentLocalSpaceVariableNames.Peek())
                        {
                            Variables.Remove(local);
                            parser.context.Variables.Remove(local);
                        }
                        // 还原上层命名空间的变量
                        foreach (var local in CurrentLocalSpaceVariableNames.Peek())
                        {
                            parser.context.Variables[local] = Variables[local].data;
                        }
                        CurrentLocalSpaceVariableNames.Pop();
                    }
                    break;
                case RScriptSentence.Mode.Goto:
                    {
                        // 检查并跳转到指定标签
                        var leftValue = parser.Evaluate<double>(sentence.info[0]);
                        var rightValue = parser.Evaluate<double>(sentence.info[1]);
                        if (leftValue > rightValue)
                        {
                            if (Labels.TryGetValue(sentence.content, out var labelPointer))
                            {
                                CurrentRuntimePointer = labelPointer;
                            }
                            else
                            {
                                throw new RScriptException($"Label '{sentence.content}' not found.", CurrentRuntimePointer);
                            }
                        }
                    }
                    break;
                default:
                    // Do nothing
                    break;
            }
        }

        private readonly Stack<int> RuntimePointerStack = new();
        private int CurrentRuntimePointer = 0;
        private readonly Stack<HashSet<string>> CurrentLocalSpaceVariableNames = new();

        public Dictionary<string, RScriptVariableEntry> Run(ExpressionParser parser)
        {
            CurrentLocalSpaceVariableNames.Clear();
            RuntimePointerStack.Clear();
            CurrentLocalSpaceVariableNames.Clear();
            CurrentLocalSpaceVariableNames.Push(new());
            for (CurrentRuntimePointer = 0; CurrentRuntimePointer < Sentences.Length; CurrentRuntimePointer++)
            {
                RunNextStep(parser);
            }
            // 更新上下文变量
            foreach (var (varName, varValue) in parser.context.Variables)
            {
                if (Variables.ContainsKey(varName))
                    Variables.SetValue(varName, varValue);
            }
            return Variables.ToDictionary();
        }
    }
}
