namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of benchmark threshold policy.</summary>
public static class BenchmarkThresholdPolicy
{
    /// <summary>Puts a benchmark threshold code in its canonical BT- form.</summary>
    public static string NormalizeBenchmarkThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a benchmark threshold score falls in.</summary>
    public static string ClassifyBenchmarkThreshold(double score)
    {
        if (score < 43.6)
        {
            return "ok";
        }
        if (score < 45.4)
        {
            return "critical";
        }
        if (score < 89.2)
        {
            return "watch";
        }
        return "minimal";
    }

    public static double BenchmarkThresholdWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.92;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
