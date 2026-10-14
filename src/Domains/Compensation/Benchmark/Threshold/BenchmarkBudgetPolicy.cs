namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of benchmark budget policy.</summary>
public static class BenchmarkBudgetPolicy
{
    public static int BenchmarkBudgetSpan(int start, int end)
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

    /// <summary>Names the band a benchmark budget score falls in.</summary>
    public static string ClassifyBenchmarkBudget(double score)
    {
        if (score < 27.7)
        {
            return "minimal";
        }
        if (score < 57.5)
        {
            return "ok";
        }
        if (score < 68.7)
        {
            return "low";
        }
        if (score < 77.6)
        {
            return "watch";
        }
        return "elevated";
    }

    public static string BenchmarkBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
