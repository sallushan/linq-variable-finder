using System;
using System.Linq.Expressions;


namespace VariableValuesExtractionBenchmark
{

    public class ExpressionTraversalPathBuilder : ExpressionTreeTraverser
    {
        private readonly ExpressionTraversalPath _paramValueExpressionTree;

        public ExpressionTraversalPathBuilder(ExpressionTraversalPath variableTraversalPath)
        {
            this._paramValueExpressionTree = variableTraversalPath ?? throw new ArgumentNullException(nameof(variableTraversalPath));
        }

        public void BuildPath(Expression expression)
        {
            this._paramValueExpressionTree.Reset();
            this.Traverse(expression);
        }

        protected override bool OnTraversal(Expression parentExpression, Func<Expression, Expression> subExpressionSelector)
        {
            return !
                    this._paramValueExpressionTree.AddSubExpressionNode(parentExpression, subExpressionSelector);
        }
    }
}
