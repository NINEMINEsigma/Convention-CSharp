using Convention.RScript.Matcher;
using Convention.RScript.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

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
            /// 跳转到指定标签, 格式: goto(boolean,labelname)
            /// <para>判断为真时跳转到labelname</para>
            /// </summary>
            Goto,
            /// <summary>
            /// 跳转到当前命名空间的结束位置, 格式: break(boolean);
            /// </summary>
            Breakpoint,
            /// <summary>
            /// 跳转到上次跳转的位置的后一个位置, 格式: back(boolean);
            /// </summary>
            Backpoint,
        }

        public string content;
        public List<string> info;
        public Mode mode;
    }

    public interface IRSentenceMatcher
    {
        bool Match(string expression, ref RScriptSentence sentence);
    }

    public partial class RScriptContext
    {
        public readonly RScriptImportClass Import;
        public readonly RScriptVariables Variables;
        private readonly RScriptSentence[] Sentences;
        private readonly Dictionary<string, int> Labels;
        private readonly Dictionary<int, int> Namespace;

        public List<IRSentenceMatcher> SentenceParser = new()
        {
            new NamespaceMater(),
            new DefineVariableMatcher(),
            new LabelMatcher(),
            new GotoMatcher(),
            new BreakMatcher(),
            new BackMatcher(),
        };

        private RScriptSentence ParseToSentence(string expression)
        {
            RScriptSentence result = new()
            {
                content = expression,
                mode = RScriptSentence.Mode.Expression
            };
            expression = expression.Trim();
            expression.TrimEnd(';');
            SentenceParser.Any(matcher => matcher.Match(expression, ref result));
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

        private void DoDefineVariable(ExpressionParser parser, RScriptSentence sentence)
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
                    varDefaultValue = new object();
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

        private void DoEnterNamespace(ExpressionParser parser)
        {
            // 准备记录当前命名空间中定义的变量, 清空上层命名空间的变量
            CurrentLocalSpaceVariableNames.Push(new());
            // 更新变量值
            foreach (var (varName, varValue) in parser.context.Variables)
            {
                Variables.SetValue(varName, varValue);
            }
            // 压栈
            RuntimePointerStack.Push(CurrentRuntimePointer);
        }

        private void DoExitNamespace(ExpressionParser parser)
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
            // 弹栈
            RuntimePointerStack.Pop();
        }

        private void DoGoto(ExpressionParser parser, RScriptSentence sentence)
        {
            // 检查并跳转到指定标签
            if (parser.Evaluate<bool>(sentence.info[0]))
            {
                if (Labels.TryGetValue(sentence.content, out var labelPointer))
                {
                    GotoPointerStack.Push(CurrentRuntimePointer);
                    CurrentRuntimePointer = labelPointer;
                }
                else
                {
                    throw new RScriptException($"Label '{sentence.content}' not found.", CurrentRuntimePointer);
                }
            }
        }

        private void DoBreakpoint(ExpressionParser parser, RScriptSentence sentence)
        {
            // 检查并跳转到当前命名空间的结束位置
            if (parser.Evaluate<bool>(sentence.content))
            {
                if (RuntimePointerStack.Count == 0)
                {
                    CurrentRuntimePointer = Sentences.Length;
                }
                else if (Namespace.TryGetValue(RuntimePointerStack.Peek(), out var exitPointer))
                {
                    CurrentRuntimePointer = exitPointer;
                    DoExitNamespace(parser);
                }
                else
                {
                    throw new NotImplementedException($"No namespace to break.");
                }
            }
        }

        private void DoBackpoint(ExpressionParser parser, RScriptSentence sentence)
        {
            // 检查并跳转到上次跳转的位置的后一个位置
            if (parser.Evaluate<bool>(sentence.content))
            {
                if (GotoPointerStack.Count == 0)
                {
                    throw new NotImplementedException($"No position to back.");
                }
                else
                {
                    CurrentRuntimePointer = GotoPointerStack.Pop() + 1;
                }
            }
        }

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
                        DoDefineVariable(parser, sentence);
                    }
                    break;
                case RScriptSentence.Mode.EnterNamespace:
                    {
                        DoEnterNamespace(parser);
                    }
                    break;
                case RScriptSentence.Mode.ExitNamespace:
                    {
                        DoExitNamespace(parser);
                    }
                    break;
                case RScriptSentence.Mode.Goto:
                    {
                        DoGoto(parser, sentence);
                    }
                    break;
                case RScriptSentence.Mode.Breakpoint:
                    {
                        DoBreakpoint(parser,sentence);
                    }
                    break;
                case RScriptSentence.Mode.Backpoint:
                    {
                        DoBackpoint(parser, sentence);
                    }
                    break;
                default:
                    // Do nothing
                    break;
            }
        }

        private readonly Stack<int> RuntimePointerStack = new();
        private readonly Stack<int> GotoPointerStack = new();
        private int CurrentRuntimePointer = 0;
        private readonly Stack<HashSet<string>> CurrentLocalSpaceVariableNames = new();

        public Dictionary<string, RScriptVariableEntry> Run(ExpressionParser parser)
        {
            CurrentLocalSpaceVariableNames.Clear();
            RuntimePointerStack.Clear();
            GotoPointerStack.Clear();
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
