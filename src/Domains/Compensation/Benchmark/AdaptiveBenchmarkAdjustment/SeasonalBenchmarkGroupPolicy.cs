namespace Acme.Workforce.Compensation.Benchmark.AdaptiveBenchmarkAdjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal benchmark group policy.</summary>
public static class SeasonalBenchmarkGroupPolicy
{
    public static double SeasonalBenchmarkGroupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.15;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a seasonal benchmark group score falls in.</summary>
    public static string ClassifySeasonalBenchmarkGroup(double score)
    {
        if (score < 16.9)
        {
            return "elevated";
        }
        if (score < 35.3)
        {
            return "minimal";
        }
        if (score < 67.2)
        {
            return "medium";
        }
        return "high";
    }
}
