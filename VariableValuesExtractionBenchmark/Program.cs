using BenchmarkDotNet.Running;
using VariableValuesExtractionBenchmark;

#if DEBUG
    var benchmark = new ExpressionValueBenchmark();
    benchmark.Setup();
#else
    BenchmarkRunner.Run<ExpressionValueBenchmark>();
#endif