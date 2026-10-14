namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of occupancy estimate policy.</summary>
public static class OccupancyEstimatePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeOccupancyEstimate()
    {
        var policy = Retry.DefaultPolicy("occupancy-estimate");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int OccupancyEstimateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 78021.0, in order.</summary>
    public static int OccupancyEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 78021.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a occupancy estimate score falls in.</summary>
    public static string ClassifyOccupancyEstimate(double score)
    {
        if (score < 32.5)
        {
            return "medium";
        }
        if (score < 77.0)
        {
            return "critical";
        }
        if (score < 79.2)
        {
            return "low";
        }
        if (score < 92.5)
        {
            return "ok";
        }
        return "high";
    }
}
