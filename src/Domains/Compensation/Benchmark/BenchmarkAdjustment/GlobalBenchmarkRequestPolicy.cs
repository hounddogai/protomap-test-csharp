namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAdjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global benchmark request policy.</summary>
public static class GlobalBenchmarkRequestPolicy
{
    public static double GlobalBenchmarkRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.06;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a global benchmark request score falls in.</summary>
    public static string ClassifyGlobalBenchmarkRequest(double score)
    {
        if (score < 40.0)
        {
            return "elevated";
        }
        if (score < 91.1)
        {
            return "ok";
        }
        if (score < 93.4)
        {
            return "high";
        }
        if (score < 98.1)
        {
            return "watch";
        }
        return "low";
    }
}
