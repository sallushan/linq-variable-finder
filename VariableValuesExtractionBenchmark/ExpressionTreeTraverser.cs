using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace VariableValuesExtractionBenchmark
{
    public abstract class ExpressionTreeTraverser
    {
        protected virtual void Traverse(Expression expression)
        {
            switch (expression)
            {
                case MethodCallExpression methodCallExpr:
                    this.TraverseMethodCallExpression(methodCallExpr);
                    break;
                case NewExpression newExpr:
                    this.TraverseNewExpression(newExpr);
                    break;
                case MemberInitExpression memberInitExpr:
                    this.TraverseMemberInitExpression(memberInitExpr);
                    break;
                case LambdaExpression lambdaExpr:
                    this.TraverseLambdaExpression(lambdaExpr);
                    break;
                case MemberExpression memberExpr:
                    this.TraverseMemberExpression(memberExpr);
                    break;
                case ConstantExpression constExpr:
                    this.TraverseConstantExpression(constExpr);
                    break;
                case BinaryExpression binExpr:
                    this.TraverseBinaryExpression(binExpr);
                    break;
                case ConditionalExpression condExpr:
                    this.TraverseConditionalExpression(condExpr);
                    break;
                case UnaryExpression unaryExpr:
                    this.TraverseUnaryExpression(unaryExpr);
                    break;
                case ParameterExpression paramExpr:
                    this.TraverseParameterExpression(paramExpr);
                    break;
                case NewArrayExpression newArrayExpr:
                    this.TraverseNewArrayExpression(newArrayExpr);
                    break;
                default:
                    throw new NotImplementedException($"Expression type '{expression.NodeType}' is not implemented");
            }
        }

        protected virtual bool OnTraversal(Expression parentExpression, Func<Expression, Expression> subExpressionSelector)
            => true;

        protected virtual void TraverseMethodCallExpression(MethodCallExpression methodCallExpr)
        {
            if (methodCallExpr.Object != null)
            {
                this.TraverseSubExpression(methodCallExpr, param_expr => ((MethodCallExpression)param_expr).Object);
            }
            for (var i = 0; i < methodCallExpr.Arguments.Count; i++)
            {
                var arr_ind = i;
                this.TraverseSubExpression(methodCallExpr, param_expr => ((MethodCallExpression)param_expr).Arguments[arr_ind]);
            }
        }

        protected virtual void TraverseNewExpression(NewExpression newExpr)
        {
            for (var i = 0; i < newExpr.Arguments.Count; i++)
            {
                var arr_ind = i;
                this.TraverseSubExpression(newExpr, param_expr => ((NewExpression)param_expr).Arguments[arr_ind]);
            }
        }

        protected virtual void TraverseMemberInitExpression(MemberInitExpression memberInitExpr)
        {
            this.TraverseSubExpression(memberInitExpr, param_expr => ((MemberInitExpression)param_expr).NewExpression);
            for (var i = 0; i < memberInitExpr.Bindings.Count; i++)
            {
                var arr_ind = i;
                this.TraverseSubExpression(memberInitExpr, param_expr => ((MemberAssignment)((MemberInitExpression)param_expr).Bindings[arr_ind]).Expression);
            }
        }

        protected virtual void TraverseLambdaExpression(LambdaExpression lambdaExpr)
        {
            this.TraverseSubExpression(lambdaExpr, param_expr => ((LambdaExpression)param_expr).Body);
        }

        protected virtual void TraverseMemberExpression(MemberExpression memberExpr)
        {
            this.TraverseSubExpression(memberExpr, param_expr => ((MemberExpression)param_expr).Expression);
        }

        protected virtual void TraverseConstantExpression(ConstantExpression constExpr)
        {
            // do nothing
        }

        protected virtual void TraverseBinaryExpression(BinaryExpression binExpr)
        {
            this.TraverseSubExpression(binExpr, param_expr => ((BinaryExpression)param_expr).Left);
            this.TraverseSubExpression(binExpr, param_expr => ((BinaryExpression)param_expr).Right);
        }

        protected virtual void TraverseConditionalExpression(ConditionalExpression condExpr)
        {
            this.TraverseSubExpression(condExpr, param_expr => ((ConditionalExpression)param_expr).Test);
            this.TraverseSubExpression(condExpr, param_expr => ((ConditionalExpression)param_expr).IfTrue);
            this.TraverseSubExpression(condExpr, param_expr => ((ConditionalExpression)param_expr).IfFalse);
        }
        protected virtual void TraverseUnaryExpression(UnaryExpression unaryExpr)
        {
            this.TraverseSubExpression(unaryExpr, param_expr => ((UnaryExpression)param_expr).Operand);
        }

        protected virtual void TraverseParameterExpression(ParameterExpression paramExpr)
        {
            // do nothing
        }

        protected virtual void TraverseNewArrayExpression(NewArrayExpression newArrayExpr)
        {
            for (var i = 0; i < newArrayExpr.Expressions.Count; i++)
            {
                var arr_ind = i;
                this.TraverseSubExpression(newArrayExpr, param_expr => ((NewArrayExpression)param_expr).Expressions[arr_ind]);
            }
        }



        protected void TraverseSubExpression(Expression parentExpression, Func<Expression, Expression> subExpressionSelector)
        {
            if (this.OnTraversal(parentExpression, subExpressionSelector))
            {
                var subExpression = subExpressionSelector(parentExpression);
                this.Traverse(subExpression);
            }
        }
    }
}
