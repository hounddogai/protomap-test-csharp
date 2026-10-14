namespace Acme.Workforce.Workplace.Occupancy.Rule;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched occupancy snapshot policy.</summary>
public static class BatchedOccupancySnapshotPolicy
{
    /// <summary>Names the band a batched occupancy snapshot score falls in.</summary>
    public static string ClassifyBatchedOccupancySnapshot(double score)
    {
        if (score < 29.0)
        {
            return "minimal";
        }
        if (score < 42.8)
        {
            return "critical";
        }
        if (score < 75.7)
        {
            return "medium";
        }
        if (score < 83.0)
        {
            return "high";
        }
        return "watch";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedOccupancySnapshot()
    {
        var policy = Retry.DefaultPolicy("batched-occupancy-snapshot");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
