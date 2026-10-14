namespace Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of global benchmark policy policy.</summary>
public static class GlobalBenchmarkPolicyPolicy
{
    public static int GlobalBenchmarkPolicySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalBenchmarkPolicy()
    {
        var policy = Retry.DefaultPolicy("global-benchmark-policy");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
