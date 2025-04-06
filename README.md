# Exploring Expression Tree Variable Extraction in .NET

This repository is a **practical study** on how to extract captured variables from LINQ expression trees in .NET — comparing traditional visitor-based traversal vs. a precomputed traversal path approach.

The goal of this project is to understand how we can make expression tree processing more efficient and how it can be plugged into larger systems, such as an ORM.

---

## Purpose

When working with LINQ expressions like:

```csharp
int minAge = 30;
Expression<Func<User, bool>> expr = x => x.Age > minAge;
```

We often need to extract the actual value (30) from inside the expression tree. While caching the translated SQL string avoids repeated query translation, we still need to re-traverse the expression tree on every execution to extract the latest values of captured variables.

This project explores how to optimize that specific part — extracting values efficiently without walking the entire tree each time.

---

## Two Approaches Compared

### Visitor-Based Traversal (Classic)

Walks the expression tree using `ExpressionVisitor`, checking each node type dynamically.

### Cached Traversal Path (Optimized)

Builds a reusable map of how to reach the variable nodes, then extracts values efficiently without re-visiting the whole tree.

---

## Benchmark Results

| Method                    | Mean     | StdDev   | Gain        |
|---------------------------|----------|----------|-------------|
| VisitorBasedExtraction    | 703.2 ns | 15.81 ns |             |
| CachedTraversalExtraction | 374.1 ns |  5.60 ns | ~47% faster |

Tested using [BenchmarkDotNet](https://benchmarkdotnet.org/) on realistic LINQ expressions with method calls, captured values, and nested paths.

---

## What's in This Repo?

| File                                | Purpose                                          |
|-------------------------------------|--------------------------------------------------|
| `ExpressionTraversalPath.cs`        | Stores precomputed traversal delegates           |
| `ExpressionTraversalPathBuilder.cs` | Builds the cached path structure                 |
| `VisitorBasedExtractor.cs`          | Classic visitor for comparison                   |
| `ReflectionService.cs`              | Resolves values from member/unary expressions    |
| `ExpressionValueBenchmark.cs`       | BenchmarkDotNet test setup                       |
| `Program.cs`                        | Entry point run and inspect                      |

---

## License

MIT License feel free to fork, study, extend, and share.

---