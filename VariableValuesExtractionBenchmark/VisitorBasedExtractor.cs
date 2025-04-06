using System.Linq.Expressions;
using System.Reflection;

namespace VariableValuesExtractionBenchmark
{
    public class VisitorBasedExtractor : ExpressionVisitor
    {
        private readonly List<object?> _values;
        private readonly IReflectionService _reflectionService;

        public VisitorBasedExtractor(IReflectionService reflectionService)
        {
            _values = new List<object?>();
            this._reflectionService = reflectionService;
        }

        public object?[] ExtractVariableValues(Expression expression)
        {
            _values.Clear();
            Visit(expression);
            return _values.ToArray();
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (this._reflectionService.IsVariableExpression(node))
            {
                this._values.Add(this._reflectionService.GetValue(node));
                return node;
            }

            return base.VisitMember(node);
        }
    }
}
