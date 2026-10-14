namespace Acme.Workforce.Compensation.Benchmark.Summary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of benchmark review policy.</summary>
public static class BenchmarkReviewPolicy
{
    public static string BenchmarkReviewBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static double BenchmarkReviewWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.33;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a benchmark review score falls in.</summary>
    public static string ClassifyBenchmarkReview(double score)
    {
        if (score < 22.3)
        {
            return "medium";
        }
        if (score < 54.7)
        {
            return "minimal";
        }
        if (score < 60.9)
        {
            return "watch";
        }
        if (score < 89.7)
        {
            return "low";
        }
        return "ok";
    }

    public static List<string> MergeBenchmarkReviewLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
