using System.Globalization;
using BenchmarkDotNet.Attributes;
using Hs.Core.Money.Extensions;

namespace Hs.Core.Money.Benchmarks;

/// <summary>The everyday operations: arithmetic, allocation, formatting and parsing.</summary>
[MemoryDiagnoser]
public class AmountBenchmarks
{
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");
    private readonly Amount _price = new(1234.56m, Currency.FromCode("USD"));
    private readonly Amount _tax = new(123.46m, Currency.FromCode("USD"));

    [Benchmark]
    public Amount Add() => _price + _tax;

    [Benchmark]
    public Amount Multiply() => _price * 1.1m;

    [Benchmark]
    public Amount[] SafeDivideInThree() => [.. _price.SafeDivide(3)];

    [Benchmark]
    public string FormatWithSymbol() => _price.ToString("C", English);

    [Benchmark]
    public string FormatInWords() => _price.ToString("W", English);

    [Benchmark]
    public Amount Parse() => Amount.Parse("1,234.56", Currency.FromCode("USD"));
}
