namespace Acme.Workforce.Compensation.Benchmark.Rule;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of benchmark mapping policy.</summary>
public static class BenchmarkMappingPolicy
{
    public static string BenchmarkMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a benchmark mapping score falls in.</summary>
    public static string ClassifyBenchmarkMapping(double score)
    {
        if (score < 38.1)
        {
            return "low";
        }
        if (score < 40.5)
        {
            return "medium";
        }
        if (score < 54.7)
        {
            return "critical";
        }
        return "minimal";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBenchmarkMapping()
    {
        var policy = Retry.DefaultPolicy("benchmark-mapping");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
