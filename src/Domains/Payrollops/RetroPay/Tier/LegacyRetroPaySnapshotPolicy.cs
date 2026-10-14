namespace Acme.Workforce.Payrollops.RetroPay.Tier;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy retro pay snapshot policy.</summary>
public static class LegacyRetroPaySnapshotPolicy
{
    public static List<string> MergeLegacyRetroPaySnapshotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyRetroPaySnapshot()
    {
        var policy = Retry.DefaultPolicy("legacy-retro-pay-snapshot");
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

    /// <summary>Names the band a legacy retro pay snapshot score falls in.</summary>
    public static string ClassifyLegacyRetroPaySnapshot(double score)
    {
        if (score < 11.5)
        {
            return "medium";
        }
        if (score < 44.0)
        {
            return "high";
        }
        return "low";
    }
}
