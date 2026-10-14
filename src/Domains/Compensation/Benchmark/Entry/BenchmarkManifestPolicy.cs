namespace Acme.Workforce.Compensation.Benchmark.Entry;

/// <summary>Functions of benchmark manifest policy.</summary>
public static class BenchmarkManifestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 21284.0, in order.</summary>
    public static int BenchmarkManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 21284.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int BenchmarkManifestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }
}
