namespace Acme.Workforce.Compensation.Benchmark.Profile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of benchmark profile policy.</summary>
public static class BenchmarkProfilePolicy
{
    /// <summary>Names the band a benchmark profile score falls in.</summary>
    public static string ClassifyBenchmarkProfile(double score)
    {
        if (score < 30.3)
        {
            return "low";
        }
        if (score < 40.1)
        {
            return "watch";
        }
        if (score < 61.7)
        {
            return "critical";
        }
        if (score < 86.8)
        {
            return "high";
        }
        return "minimal";
    }

    public static int BenchmarkProfileSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    public static string BenchmarkProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static List<string> MergeBenchmarkProfileLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
