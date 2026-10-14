namespace Acme.Workforce.Workplace.Occupancy.Target;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of occupancy rollup policy.</summary>
public static class OccupancyRollupPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeOccupancyRollup()
    {
        var policy = Retry.DefaultPolicy("occupancy-rollup");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a occupancy rollup score falls in.</summary>
    public static string ClassifyOccupancyRollup(double score)
    {
        if (score < 33.1)
        {
            return "low";
        }
        if (score < 66.2)
        {
            return "elevated";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 24915.0, in order.</summary>
    public static int OccupancyRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 24915.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int OccupancyRollupSpan(int start, int end)
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
