namespace Acme.Workforce.Compensation.Benchmark.Metric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual benchmark metric policy.</summary>
public static class ManualBenchmarkMetricPolicy
{
    /// <summary>Names the band a manual benchmark metric score falls in.</summary>
    public static string ClassifyManualBenchmarkMetric(double score)
    {
        if (score < 33.2)
        {
            return "critical";
        }
        if (score < 33.4)
        {
            return "high";
        }
        if (score < 91.6)
        {
            return "elevated";
        }
        return "minimal";
    }

    /// <summary>Puts a manual benchmark metric code in its canonical MBM- form.</summary>
    public static string NormalizeManualBenchmarkMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MBM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MBM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string ManualBenchmarkMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static List<string> MergeManualBenchmarkMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
