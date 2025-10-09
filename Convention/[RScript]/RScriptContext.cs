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
            /// 标签, 格式: label: 标签名
            /// </summary>
            Label,
            /// <summary>
            /// 跳转到指定标签, 格式: goto 标签名
            /// </summary>
            Goto,
            /// <summary>
            /// 条件判断, 格式: if (条件表达式)
            /// </summary>
            If
        }

        public string content;
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

            Regex DefineVariableRegex = new(@"^(string|int|double|float|bool|var) [a-zA-Z_][a-zA-Z0-9_]*$");
            var DefineVariableMatch = DefineVariableRegex.Match(expression);
            if (DefineVariableMatch.Success)
            {
                result.mode = RScriptSentence.Mode.DefineVariable;
                return result;
            }

            Regex LabelRegex = new(@"^label:\s*([a-zA-Z_][a-zA-Z0-9_]*)$");
            var LabelMatch = LabelRegex.Match(expression);
            if (LabelMatch.Success)
            {
                result.mode = RScriptSentence.Mode.Label;
                result.content = LabelMatch.Groups[1].Value;
                return result;
            }

            Regex GotoRegex = new(@"^goto\s+([a-zA-Z_][a-zA-Z0-9_]*)$");
            var GotoMatch = GotoRegex.Match(expression);
            if (GotoMatch.Success)
            {
                result.mode = RScriptSentence.Mode.Goto;
                result.content = GotoMatch.Groups[1].Value;
                return result;
            }

            Regex IfRegex = new(@"^if\s*\((.*)\)$");
            var IfMatch = IfRegex.Match(expression);
            if (IfMatch.Success)
            {
                result.mode = RScriptSentence.Mode.If;
                result.content = IfMatch.Groups[1].Value;
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
                        throw new RScriptExceptionException("Namespace exit without enter.", i);
                    var enterPointer = namespaceLayers.Pop();
                    namespaceIndicator[enterPointer] = i;
                }
            }
            if (namespaceLayers.Count > 0)
            {
                throw new RScriptExceptionException("Namespace enter without exit.", namespaceLayers.Peek());
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
                        var content = sentence.content.Split(' ');
                        Type varType;
                        object varDefaultValue;
                        {
                            if (content[0] == "string")
                            {
                                varType = typeof(string);
                                varDefaultValue = string.Empty;
                            }
                            else if (content[0] == "int")
                            {
                                varType = typeof(int);
                                varDefaultValue = 0;
                            }
                            else if (content[0] == "double")
                            {
                                varType = typeof(double);
                                varDefaultValue = 0.0;
                            }
                            else if (content[0] == "float")
                            {
                                varType = typeof(float);
                                varDefaultValue = 0.0f;
                            }
                            else if (content[0] == "bool")
                            {
                                varType = typeof(bool);
                                varDefaultValue = false;
                            }
                            else if (content[0] == "var")
                            {
                                varType = typeof(object);
                                varDefaultValue = null;
                            }
                            else
                            {
                                throw new RScriptExceptionException($"Unsupported variable type '{content[0]}'.", CurrentRuntimePointer);
                            }
                        }
                        var varName = content[1];
                        if (CurrentLocalSpaceVariableNames.Contains(varName) == false)
                        {
                            Variables.Add(varName, new() { type = varType, data = varDefaultValue });
                            parser.context.Variables[varName] = varDefaultValue;
                            CurrentLocalSpaceVariableNames.Add(varName);
                        }
                        else
                        {
                            throw new RScriptExceptionException($"Variable '{varName}' already defined on this namespace.", CurrentRuntimePointer);
                        }
                    }
                    break;
                case RScriptSentence.Mode.EnterNamespace:
                    {
                        // 准备记录当前命名空间中定义的变量, 清空上层命名空间的变量
                        CurrentLocalSpaceVariableNames.Clear();
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
                        foreach (var local in CurrentLocalSpaceVariableNames)
                        {
                            Variables.Remove(local);
                            parser.context.Variables.Remove(local);
                        }
                        CurrentLocalSpaceVariableNames.Clear();
                        // 还原上层命名空间的变量
                        foreach (var local in Variables.Keys)
                        {
                            CurrentLocalSpaceVariableNames.Add(local);
                            parser.context.Variables[local] = Variables[local].data;
                        }
                    }
                    break;
                case RScriptSentence.Mode.Goto:
                    {
                        // 跳转到指定标签
                        if (Labels.TryGetValue(sentence.content, out var labelPointer))
                        {
                            CurrentRuntimePointer = labelPointer;
                        }
                        else
                        {
                            throw new RScriptExceptionException($"Label '{sentence.content}' not found.", CurrentRuntimePointer);
                        }
                    }
                    break;
                case RScriptSentence.Mode.If:
                    {
                        // 条件跳转
                        var conditionResult = parser.Evaluate(sentence.content);
                        if (conditionResult is bool b)
                        {
                            if (b == false)
                            {
                                if (Namespace.TryGetValue(CurrentRuntimePointer + 1, out var exitPointer) == false)
                                {
                                    // 没有命名空间时只跳过下一句, +1后在外层循环末尾再+1, 最终结果为下一次循环开始时已经指向第二句
                                    exitPointer = CurrentRuntimePointer + 1;
                                }
                                CurrentRuntimePointer = exitPointer;
                            }
                        }
                        else
                        {
                            throw new RScriptExceptionException($"If condition must be bool, but got {conditionResult?.GetType().ToString() ?? "null"}.", CurrentRuntimePointer);
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
        private readonly HashSet<string> CurrentLocalSpaceVariableNames = new();

        public Dictionary<string, RScriptVariableEntry> Run(ExpressionParser parser)
        {
            CurrentLocalSpaceVariableNames.Clear();
            RuntimePointerStack.Clear();
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
