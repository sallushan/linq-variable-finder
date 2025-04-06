using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VariableValuesExtractionBenchmark
{
    public class ReflectionService : IReflectionService
    {
        public bool IsVariableExpression(Expression expression)
        {
            return IsVariableExpressionInternal(expression, 0);
        }

        private static bool IsVariableExpressionInternal(Expression expression, int depth = 0)
        {
            return expression switch
            {
                MemberExpression member => member.Expression == null || IsVariableExpressionInternal(member.Expression, depth + 1),
                ConstantExpression => depth > 0,
                BinaryExpression binary when binary.NodeType == ExpressionType.ArrayIndex =>
                    IsVariableExpressionInternal(binary.Left, depth + 1) &&
                    IsVariableExpressionInternal(binary.Right, depth + 1),
                _ => false
            };
        }

        public object? GetValue(Expression expression)
        {
            if (expression is ConstantExpression ce)
                return ce.Value;

            if (expression is MemberExpression me && me.Expression is ConstantExpression ce2)
            {
                var container = ce2.Value;
                return me.Member switch
                {
                    FieldInfo fi => fi.GetValue(container),
                    PropertyInfo pi => pi.GetValue(container),
                    _ => null
                };
            }

            return null;
        }
    }


}
