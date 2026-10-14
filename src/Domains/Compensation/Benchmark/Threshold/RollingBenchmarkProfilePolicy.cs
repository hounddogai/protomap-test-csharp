namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling benchmark profile policy.</summary>
public static class RollingBenchmarkProfilePolicy
{
    public static double RollingBenchmarkProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.2;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a rolling benchmark profile score falls in.</summary>
    public static string ClassifyRollingBenchmarkProfile(double score)
    {
        if (score < 17.0)
        {
            return "low";
        }
        if (score < 26.2)
        {
            return "minimal";
        }
        if (score < 30.1)
        {
            return "watch";
        }
        if (score < 84.9)
        {
            return "high";
        }
        return "elevated";
    }
}
