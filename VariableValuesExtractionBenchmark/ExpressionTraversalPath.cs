using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Linq.Expressions;

namespace VariableValuesExtractionBenchmark
{
    public class ExpressionTraversalPath
    {
        private readonly IReflectionService _reflectionService;

        private class ExpressionNode
        {
            public Func<Expression, Expression> Extractor { get; }
            public List<ExpressionNode> Children { get; }
            public bool IsVariable { get; }
            private readonly IReflectionService _reflectionService;

            public ExpressionNode(IReflectionService reflectionService, Func<Expression, Expression> extractor, bool isVariable)
            {
                _reflectionService = reflectionService;
                Extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
                IsVariable = isVariable;
                Children = new List<ExpressionNode>();
            }

            public void ExtractValue(Expression expression, List<object?> values)
            {
                if (IsVariable)
                    values.Add(_reflectionService.GetValue(expression));
            }
        }

        private readonly Dictionary<Expression, ExpressionNode> _expressionToNode = new();
        private readonly Dictionary<ExpressionNode, Expression> _nodeToExpression = new();
        private ExpressionNode? _rootNode;
        private bool _pruned;

        public ExpressionTraversalPath(IReflectionService reflectionService)
        {
            _reflectionService = reflectionService ?? throw new ArgumentNullException(nameof(reflectionService));
        }

        public void Reset()
        {
            _expressionToNode.Clear();
            _nodeToExpression.Clear();
            _rootNode = null;
            _pruned = false;
        }

        public bool AddSubExpressionNode(Expression parentExpression, Func<Expression, Expression> extractor)
        {
            if (_rootNode == null)
            {
                _rootNode = new ExpressionNode(_reflectionService, x => x, false);
                RegisterNode(_rootNode, parentExpression);
            }

            if (!_expressionToNode.TryGetValue(parentExpression, out var parentNode))
                throw new InvalidOperationException($"Parent expression not registered: {parentExpression}");

            var childExpr = extractor(parentExpression);
            bool isVariable = _reflectionService.IsVariableExpression(childExpr);

            var childNode = new ExpressionNode(_reflectionService, extractor, isVariable);
            parentNode.Children.Add(childNode);
            RegisterNode(childNode, childExpr);

            return isVariable;
        }

        public object?[] ExtractVariableValues(Expression rootExpression)
        {
            if (rootExpression == null)
                throw new ArgumentNullException(nameof(rootExpression));

            if (_rootNode == null)
                return Array.Empty<object?>();

            if (!_pruned)
            {
                PruneNonVariableNodes(_rootNode);
                _pruned = true;
            }

            var values = new List<object?>();
            Traverse(_rootNode, rootExpression, values);
            return values.ToArray();
        }

        private void Traverse(ExpressionNode node, Expression expr, List<object?> result)
        {
            foreach (var child in node.Children)
            {
                var subExpr = child.Extractor(expr);
                Traverse(child, subExpr, result);
            }

            node.ExtractValue(expr, result);
        }

        private void PruneNonVariableNodes(ExpressionNode node)
        {
            node.Children.RemoveAll(child =>
            {
                PruneNonVariableNodes(child);
                bool removable = !child.IsVariable && child.Children.Count == 0;

                if (removable && _nodeToExpression.TryGetValue(child, out var expr))
                    _expressionToNode.Remove(expr);

                _nodeToExpression.Remove(child);
                return removable;
            });
        }

        private void RegisterNode(ExpressionNode node, Expression expr)
        {
            _expressionToNode[expr] = node;
            _nodeToExpression[node] = expr;
        }

        public bool IsVariableNode(Expression expression)
        {
            return _reflectionService.IsVariableExpression(expression);
        }

    }
}
