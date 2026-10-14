namespace Acme.Workforce.Compensation.Benchmark.Entry;

/// <summary>Functions of benchmark entry policy.</summary>
public static class BenchmarkEntryPolicy
{
    /// <summary>Names the band a benchmark entry score falls in.</summary>
    public static string ClassifyBenchmarkEntry(double score)
    {
        if (score < 27.6)
        {
            return "medium";
        }
        if (score < 90.8)
        {
            return "elevated";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 36220.0, in order.</summary>
    public static int BenchmarkEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 36220.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
