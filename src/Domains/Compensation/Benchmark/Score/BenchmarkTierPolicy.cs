namespace Acme.Workforce.Compensation.Benchmark.Score;

/// <summary>Functions of benchmark tier policy.</summary>
public static class BenchmarkTierPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 14699.0, in order.</summary>
    public static int BenchmarkTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 14699.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a benchmark tier score falls in.</summary>
    public static string ClassifyBenchmarkTier(double score)
    {
        if (score < 47.6)
        {
            return "watch";
        }
        if (score < 79.4)
        {
            return "high";
        }
        return "medium";
    }
}
