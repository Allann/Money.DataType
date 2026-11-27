using BenchmarkDotNet.Attributes;

namespace Hs.Core.Money.Benchmarks;

/// <summary>Currency lookup, which every Amount constructed from a code pays for.</summary>
[MemoryDiagnoser]
public class CurrencyBenchmarks
{
    [Benchmark(Baseline = true)]
    public Currency CurrentIsoCode() => Currency.FromCode("EUR");

    [Benchmark]
    public Currency HistoricIsoCode() => Currency.FromCode("ARY");

    [Benchmark]
    public Currency CodeInNamespace() => Currency.FromCode("EUR", "ISO-4217");
}
