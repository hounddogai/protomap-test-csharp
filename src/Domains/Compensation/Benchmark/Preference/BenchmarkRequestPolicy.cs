namespace Acme.Workforce.Compensation.Benchmark.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of benchmark request policy.</summary>
public static class BenchmarkRequestPolicy
{
    public static string BenchmarkRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static int BenchmarkRequestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Puts a benchmark request code in its canonical BR- form.</summary>
    public static string NormalizeBenchmarkRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a benchmark request score falls in.</summary>
    public static string ClassifyBenchmarkRequest(double score)
    {
        if (score < 36.4)
        {
            return "medium";
        }
        if (score < 52.9)
        {
            return "low";
        }
        if (score < 94.1)
        {
            return "critical";
        }
        return "watch";
    }
}
