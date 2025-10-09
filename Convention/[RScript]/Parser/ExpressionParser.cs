using Flee.PublicTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convention.RScript.Parser
{
    public static class ExpressionExtension
    {
        public const double DefaultDoubleAccuracy = 1e-7;

        public static bool IsClose(this double value1, double value2, double maximumAbsoluteError = DefaultDoubleAccuracy)
        {
            if (double.IsInfinity(value1) || double.IsInfinity(value2))
            {
                return Equals(value1, value2);
            }

            if (double.IsNaN(value1) || double.IsNaN(value2))
            {
                return false;
            }

            var delta = value1 - value2;
            return !(delta > maximumAbsoluteError || delta < -maximumAbsoluteError);
        }

        public static bool IsCloseToZero(this double value, double maximumAbsoluteError = DefaultDoubleAccuracy)
        {
            return !(double.IsInfinity(value) || double.IsNaN(value) || value > maximumAbsoluteError || value < -maximumAbsoluteError);
        }
    }

    public class ExpressionParser
    {
        public readonly ExpressionContext context;

        public ExpressionParser(ExpressionContext context)
        {
            this.context = context;
        }

        private readonly Dictionary<string, IExpression> CompileGenericExpression = new();
        private readonly Dictionary<string, IDynamicExpression> CompileDynamicExpression = new();

        public void ClearCache()
        {
            CompileGenericExpression.Clear();
            CompileDynamicExpression.Clear();
        }

        public T Evaluate<T>(string expression)
        {
            if (CompileGenericExpression.TryGetValue(expression, out var result))
            {
                return (result as IGenericExpression<T>).Evaluate();
            }
            var compile = context.CompileGeneric<T>(expression);
            CompileGenericExpression[expression] = compile;
            return compile.Evaluate();
        }

        public object Evaluate(string expression)
        {
            if (CompileDynamicExpression.TryGetValue(expression, out var result))
            {
                return result.Evaluate();
            }
            var compile = context.CompileDynamic(expression);
            CompileDynamicExpression[expression] = compile;
            return compile.Evaluate();
        }
    }
}
