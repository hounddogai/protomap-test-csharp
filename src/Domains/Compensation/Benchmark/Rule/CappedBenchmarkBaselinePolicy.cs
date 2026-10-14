namespace Acme.Workforce.Compensation.Benchmark.Rule;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped benchmark baseline policy.</summary>
public static class CappedBenchmarkBaselinePolicy
{
    public static string CappedBenchmarkBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedBenchmarkBaseline()
    {
        var policy = Retry.DefaultPolicy("capped-benchmark-baseline");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
