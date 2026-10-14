namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched leave request snapshot policy.</summary>
public static class BatchedLeaveRequestSnapshotPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedLeaveRequestSnapshot()
    {
        var policy = Retry.DefaultPolicy("batched-leave-request-snapshot");
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

    public static string BatchedLeaveRequestSnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a batched leave request snapshot score falls in.</summary>
    public static string ClassifyBatchedLeaveRequestSnapshot(double score)
    {
        if (score < 73.6)
        {
            return "medium";
        }
        if (score < 83.1)
        {
            return "low";
        }
        if (score < 94.7)
        {
            return "watch";
        }
        return "elevated";
    }
}
