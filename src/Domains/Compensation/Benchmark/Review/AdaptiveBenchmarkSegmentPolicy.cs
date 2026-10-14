namespace Acme.Workforce.Compensation.Benchmark.Review;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive benchmark segment policy.</summary>
public static class AdaptiveBenchmarkSegmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveBenchmarkSegment()
    {
        var policy = Retry.DefaultPolicy("adaptive-benchmark-segment");
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

    /// <summary>Names the band a adaptive benchmark segment score falls in.</summary>
    public static string ClassifyAdaptiveBenchmarkSegment(double score)
    {
        if (score < 11.1)
        {
            return "high";
        }
        if (score < 11.8)
        {
            return "ok";
        }
        return "watch";
    }

    /// <summary>Puts a adaptive benchmark segment code in its canonical ABS- form.</summary>
    public static string NormalizeAdaptiveBenchmarkSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ABS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ABS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
