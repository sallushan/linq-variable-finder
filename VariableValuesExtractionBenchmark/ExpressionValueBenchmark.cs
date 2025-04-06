using BenchmarkDotNet.Attributes;
using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace VariableValuesExtractionBenchmark
{
    public class ExpressionValueBenchmark
    {
        private Expression? _expression;

        private ExpressionTraversalPath _cachedTree = null!;
        private ReflectionService _reflectionService = null!;
        private VisitorBasedExtractor _visitorBasedExtractor = null!;

        [GlobalSetup]
        public void Setup()
        {
            int minAge = 30;
            int maxAge = 50;
            string namePrefix = "Engr.";
            string[] departments = { "HR", "Finance", "Legal" };
            DateTime cutoff = DateTime.Now.AddYears(-3);

            var dummyEntities = new List<DummyEntity>().AsQueryable();

            var q = dummyEntities
                .Where(x => x.Age >= minAge)
                .Where(x => x.Age <= maxAge)
                .Where(x => x.Name.StartsWith(namePrefix))
                .Where(x => x.IsActive || x.Address.Country == "Pakistan")
                .Where(x => x.JoinDate < cutoff)
                .Select(x => new
                {
                    FullName = namePrefix + x.Name,
                    Dept = x.Department.Name,
                    Country = x.Address.Country
                })
                .OrderBy(x => x.FullName.Length);

            _expression = q.Expression;

            _reflectionService = new ReflectionService();
            _visitorBasedExtractor = new VisitorBasedExtractor(_reflectionService);
            _cachedTree = new ExpressionTraversalPath(_reflectionService);

            var tracker = new ExpressionTraversalPathBuilder(_cachedTree);
            tracker.BuildPath(_expression);

            ValidateFunctionality();
        }

        private void ValidateFunctionality()
        {
            // Extract values from both methods
            var visitorValues = _visitorBasedExtractor.ExtractVariableValues(_expression!);
            var cachedValues = _cachedTree.ExtractVariableValues(_expression!);

            // Sanity check
            if (visitorValues.Length != cachedValues.Length ||
                !visitorValues.SequenceEqual(cachedValues))
            {
                Console.WriteLine("Value extraction mismatch between Visitor and Cached traversal!");
                Console.WriteLine("Visitor Values:");
                foreach (var v in visitorValues) Console.WriteLine($"  - {v}");
                Console.WriteLine("Cached Values:");
                foreach (var v in cachedValues) Console.WriteLine($"  - {v}");

                throw new InvalidOperationException("Mismatch between extraction methods.");
            }

            foreach (var value in cachedValues)
            {
                Console.WriteLine(value ?? "(null)");
            }

            Console.WriteLine("Extraction verification passed.");
        }

        [Benchmark]
        public void VisitorBasedExtraction()
        {
            _ = _visitorBasedExtractor.ExtractVariableValues(_expression!);
        }

        [Benchmark]
        public void CachedTraversalExtraction()
        {
            _ = _cachedTree.ExtractVariableValues(_expression!);
        }

        public class DummyEntity
        {
            public int Age { get; set; }
            public string Name { get; set; } = "";
            public Address Address { get; set; } = new();
            public bool IsActive { get; set; }
            public DateTime JoinDate { get; set; }
            public Department Department { get; set; } = new();
        }

        public class Address
        {
            public string Country { get; set; } = "";
        }

        public class Department
        {
            public string Name { get; set; } = "";
        }


    }
}