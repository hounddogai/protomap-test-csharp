namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling benchmark summary policy.</summary>
public static class RollingBenchmarkSummaryPolicy
{
    public static string RollingBenchmarkSummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 54238.0, in order.</summary>
    public static int RollingBenchmarkSummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 54238.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a rolling benchmark summary score falls in.</summary>
    public static string ClassifyRollingBenchmarkSummary(double score)
    {
        if (score < 49.7)
        {
            return "minimal";
        }
        if (score < 74.4)
        {
            return "ok";
        }
        return "watch";
    }
}
