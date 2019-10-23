using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Nemesis.Wpf.Tests
{
#if NETCOREAPP3_0
    /// <![CDATA[Perf tests:
    /// //var summary = BenchmarkRunner.Run<Bench>();
    /// [MemoryDiagnoser]
    /// public class Bench
    /// {
    ///     private const string TEST = "7xAuto@Key1,15,3*,*,,18.8@Key2, 3.5 *@Key3,8x*";
    ///  
    ///     private readonly (GridLength Length, string SharedSizeGroup, byte Count)[] _resultStandard = new (GridLength Length, string SharedSizeGroup, byte Count)[8];
    ///     private readonly (GridLength Length, string SharedSizeGroup, byte Count)[] _resultSpan = new (GridLength Length, string SharedSizeGroup, byte Count)[8];
    ///  
    ///     private readonly StandardCellDefinitionParser _standardParser = new StandardCellDefinitionParser();
    ///  
    ///     [Benchmark]
    ///     public void StandardParsing()
    ///     {
    ///         int i = 0;
    ///         foreach (var definition in _standardParser.ParseDefinitions(TEST))
    ///             _resultStandard[i++] = definition;
    ///     }
    ///  
    ///     [Benchmark]
    ///     public void SpanParsing()
    ///     {
    ///         int i = 0;
    ///         foreach (var definition in SpanCellDefinitionParser.ParseDefinitions(TEST))
    ///             _resultSpan[i++] = definition;
    ///     }
    /// }
    /// ]]>
    [TestFixture]
    internal class SpanCellDefinitionParserTexts
    {
        [TestCase("7xAuto@Key1,15,3*,*,,18.8@Key2, 3.5 *,8x*")]
        [TestCase(",,")]
        [TestCase(",*,")]
        [TestCase(",Auto,*,")]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(" , ")]
        [TestCase(" ,")]
        [TestCase("Auto")]
        [TestCase("Auto,")]
        public void ConformityTestsWithStandard(string text)
        {
            var expected = StandardCellDefinitionParser.Instance.ParseDefinitions(text).ToList();

            List<(GridLength Length, string SharedSizeGroup, byte Count)> actual = new List<(GridLength Length, string SharedSizeGroup, byte Count)>();
            foreach (var definition in SpanCellDefinitionParser.ParseDefinitions(text))
                actual.Add(definition);

            Assert.That(actual, Is.EquivalentTo(expected));
        }
    }
#endif
}
