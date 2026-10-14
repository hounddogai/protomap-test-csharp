namespace Acme.Workforce.Compensation.Benchmark.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional benchmark estimate policy.</summary>
public static class RegionalBenchmarkEstimatePolicy
{
    public static string RegionalBenchmarkEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a regional benchmark estimate score falls in.</summary>
    public static string ClassifyRegionalBenchmarkEstimate(double score)
    {
        if (score < 14.6)
        {
            return "low";
        }
        if (score < 34.9)
        {
            return "watch";
        }
        if (score < 55.2)
        {
            return "minimal";
        }
        return "ok";
    }
}
