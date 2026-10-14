namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of mobility snapshot policy.</summary>
public static class MobilitySnapshotPolicy
{
    public static string MobilitySnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a mobility snapshot score falls in.</summary>
    public static string ClassifyMobilitySnapshot(double score)
    {
        if (score < 3.0)
        {
            return "medium";
        }
        if (score < 61.1)
        {
            return "elevated";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeMobilitySnapshot()
    {
        var policy = Retry.DefaultPolicy("mobility-snapshot");
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
