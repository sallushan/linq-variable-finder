using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace VariableValuesExtractionBenchmark
{
    public interface IReflectionService
    {
        bool IsVariableExpression(Expression expression);
        object? GetValue(Expression expression);

    }
}
