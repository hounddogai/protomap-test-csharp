namespace Acme.Workforce.Compensation.Benchmark.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched benchmark snapshot policy.</summary>
public static class BatchedBenchmarkSnapshotPolicy
{
    public static double BatchedBenchmarkSnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.41;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int BatchedBenchmarkSnapshotSpan(int start, int end)
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

    /// <summary>Counts how many amounts fit in a budget of 13669.0, in order.</summary>
    public static int BatchedBenchmarkSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 13669.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
