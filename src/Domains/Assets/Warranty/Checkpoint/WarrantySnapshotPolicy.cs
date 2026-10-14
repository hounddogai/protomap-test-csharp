namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of warranty snapshot policy.</summary>
public static class WarrantySnapshotPolicy
{
    /// <summary>Names the band a warranty snapshot score falls in.</summary>
    public static string ClassifyWarrantySnapshot(double score)
    {
        if (score < 11.5)
        {
            return "minimal";
        }
        if (score < 40.8)
        {
            return "elevated";
        }
        if (score < 74.1)
        {
            return "ok";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWarrantySnapshot()
    {
        var policy = Retry.DefaultPolicy("warranty-snapshot");
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

    public static string WarrantySnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
