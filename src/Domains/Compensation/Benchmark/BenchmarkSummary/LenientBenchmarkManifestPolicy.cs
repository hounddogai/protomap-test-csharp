namespace Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient benchmark manifest policy.</summary>
public static class LenientBenchmarkManifestPolicy
{
    /// <summary>Names the band a lenient benchmark manifest score falls in.</summary>
    public static string ClassifyLenientBenchmarkManifest(double score)
    {
        if (score < 2.6)
        {
            return "medium";
        }
        if (score < 62.5)
        {
            return "minimal";
        }
        if (score < 73.3)
        {
            return "critical";
        }
        return "ok";
    }

    public static string LenientBenchmarkManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static int LenientBenchmarkManifestSpan(int start, int end)
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
        return span / 86400;
    }

    public static List<string> MergeLenientBenchmarkManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
