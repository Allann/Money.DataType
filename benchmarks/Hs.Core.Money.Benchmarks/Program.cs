using BenchmarkDotNet.Running;

// dotnet run -c Release --project benchmarks/Hs.Core.Money.Benchmarks -- --filter '*'
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
