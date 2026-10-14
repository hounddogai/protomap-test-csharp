namespace Acme.Workforce.Compensation.Benchmark.Score;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered benchmark score policy.</summary>
public static class TieredBenchmarkScorePolicy
{
    /// <summary>Puts a tiered benchmark score code in its canonical TBS- form.</summary>
    public static string NormalizeTieredBenchmarkScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TBS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TBS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredBenchmarkScore()
    {
        var policy = Retry.DefaultPolicy("tiered-benchmark-score");
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
