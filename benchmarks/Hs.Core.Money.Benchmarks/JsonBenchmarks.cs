using BenchmarkDotNet.Attributes;
using Hs.Core.Money.Extensions;

namespace Hs.Core.Money.Benchmarks;

/// <summary>Round-tripping an amount through the JSON helpers.</summary>
[MemoryDiagnoser]
public class JsonBenchmarks
{
    private const string Json = """{"value":1234.56,"currency":"EUR"}""";
    private readonly Amount _amount = new(1234.56m, Currency.FromCode("EUR"));

    [Benchmark]
    public string Serialize() => _amount.ToJson();

    [Benchmark]
    public Amount Deserialize() => Json.FromJson<Amount>();
}
