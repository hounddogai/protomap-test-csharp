namespace Acme.Workforce.Compensation.Benchmark.Metric;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive benchmark metric policy.</summary>
public static class AdaptiveBenchmarkMetricPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveBenchmarkMetric()
    {
        var policy = Retry.DefaultPolicy("adaptive-benchmark-metric");
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

    public static int AdaptiveBenchmarkMetricSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a adaptive benchmark metric code in its canonical ABM- form.</summary>
    public static string NormalizeAdaptiveBenchmarkMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ABM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ABM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
