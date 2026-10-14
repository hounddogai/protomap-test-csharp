namespace Acme.Workforce.Compensation.Benchmark.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional benchmark review policy.</summary>
public static class RegionalBenchmarkReviewPolicy
{
    /// <summary>Names the band a regional benchmark review score falls in.</summary>
    public static string ClassifyRegionalBenchmarkReview(double score)
    {
        if (score < 1.6)
        {
            return "low";
        }
        if (score < 91.1)
        {
            return "high";
        }
        return "critical";
    }

    public static double RegionalBenchmarkReviewWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.9;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string RegionalBenchmarkReviewBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static int RegionalBenchmarkReviewSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }
}
