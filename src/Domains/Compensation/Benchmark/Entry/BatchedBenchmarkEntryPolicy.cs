namespace Acme.Workforce.Compensation.Benchmark.Entry;

/// <summary>Functions of batched benchmark entry policy.</summary>
public static class BatchedBenchmarkEntryPolicy
{
    public static int BatchedBenchmarkEntrySpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 41564.0, in order.</summary>
    public static int BatchedBenchmarkEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 41564.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
