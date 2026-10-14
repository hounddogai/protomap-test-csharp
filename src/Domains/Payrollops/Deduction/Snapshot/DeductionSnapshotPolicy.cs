namespace Acme.Workforce.Payrollops.Deduction.Snapshot;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of deduction snapshot policy.</summary>
public static class DeductionSnapshotPolicy
{
    public static string DeductionSnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a deduction snapshot score falls in.</summary>
    public static string ClassifyDeductionSnapshot(double score)
    {
        if (score < 28.2)
        {
            return "high";
        }
        if (score < 67.0)
        {
            return "low";
        }
        return "minimal";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDeductionSnapshot()
    {
        var policy = Retry.DefaultPolicy("deduction-snapshot");
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
}
