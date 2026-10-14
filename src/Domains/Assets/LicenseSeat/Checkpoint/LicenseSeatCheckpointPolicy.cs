namespace Acme.Workforce.Assets.LicenseSeat.Checkpoint;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of license seat checkpoint policy.</summary>
public static class LicenseSeatCheckpointPolicy
{
    /// <summary>Names the band a license seat checkpoint score falls in.</summary>
    public static string ClassifyLicenseSeatCheckpoint(double score)
    {
        if (score < 64.6)
        {
            return "watch";
        }
        if (score < 68.6)
        {
            return "high";
        }
        if (score < 70.7)
        {
            return "medium";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLicenseSeatCheckpoint()
    {
        var policy = Retry.DefaultPolicy("license-seat-checkpoint");
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

    public static List<string> MergeLicenseSeatCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
