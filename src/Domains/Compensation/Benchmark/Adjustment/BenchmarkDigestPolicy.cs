namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of benchmark digest policy.</summary>
public static class BenchmarkDigestPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBenchmarkDigest()
    {
        var policy = Retry.DefaultPolicy("benchmark-digest");
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

    /// <summary>Puts a benchmark digest code in its canonical BD- form.</summary>
    public static string NormalizeBenchmarkDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
