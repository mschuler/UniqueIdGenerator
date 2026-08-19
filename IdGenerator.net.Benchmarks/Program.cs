using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using UniqueIdGenerator.Net;

BenchmarkRunner.Run<GeneratorBenchmarks>();

[MemoryDiagnoser]
[MediumRunJob]
public class GeneratorBenchmarks
{
    private readonly Generator _generator = new(0, DateTime.UnixEpoch);

    [Benchmark]
    public string Next()
    {
        return _generator.Next();
    }

    [Benchmark]
    public ulong NextLong()
    {
        return _generator.NextLong();
    }

}
