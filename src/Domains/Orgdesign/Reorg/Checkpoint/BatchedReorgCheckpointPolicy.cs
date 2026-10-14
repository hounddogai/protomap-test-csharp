namespace Acme.Workforce.Orgdesign.Reorg.Checkpoint;

/// <summary>Functions of batched reorg checkpoint policy.</summary>
public static class BatchedReorgCheckpointPolicy
{
    public static int BatchedReorgCheckpointSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 93880.0, in order.</summary>
    public static int BatchedReorgCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 93880.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
