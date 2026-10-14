namespace Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal benchmark digest policy.</summary>
public static class SeasonalBenchmarkDigestPolicy
{
    /// <summary>Puts a seasonal benchmark digest code in its canonical SBD- form.</summary>
    public static string NormalizeSeasonalBenchmarkDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SBD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SBD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalBenchmarkDigest()
    {
        var policy = Retry.DefaultPolicy("seasonal-benchmark-digest");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
