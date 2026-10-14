namespace Acme.Workforce.Compensation.Benchmark.Checkpoint;

/// <summary>Functions of capped benchmark checkpoint policy.</summary>
public static class CappedBenchmarkCheckpointPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 67506.0, in order.</summary>
    public static int CappedBenchmarkCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 67506.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int CappedBenchmarkCheckpointSpan(int start, int end)
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
        return span / 3600;
    }
}
