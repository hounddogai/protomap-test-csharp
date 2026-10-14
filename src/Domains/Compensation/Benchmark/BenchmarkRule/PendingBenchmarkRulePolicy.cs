namespace Acme.Workforce.Compensation.Benchmark.BenchmarkRule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending benchmark rule policy.</summary>
public static class PendingBenchmarkRulePolicy
{
    public static int PendingBenchmarkRuleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    public static double PendingBenchmarkRuleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.39;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a pending benchmark rule score falls in.</summary>
    public static string ClassifyPendingBenchmarkRule(double score)
    {
        if (score < 6.4)
        {
            return "watch";
        }
        if (score < 85.9)
        {
            return "high";
        }
        return "elevated";
    }
}
