namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal policy ack entry policy.</summary>
public static class SeasonalPolicyAckEntryPolicy
{
    public static List<string> MergeSeasonalPolicyAckEntryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a seasonal policy ack entry score falls in.</summary>
    public static string ClassifySeasonalPolicyAckEntry(double score)
    {
        if (score < 15.7)
        {
            return "low";
        }
        if (score < 52.1)
        {
            return "minimal";
        }
        if (score < 69.9)
        {
            return "elevated";
        }
        if (score < 83.5)
        {
            return "high";
        }
        return "watch";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalPolicyAckEntry()
    {
        var policy = Retry.DefaultPolicy("seasonal-policy-ack-entry");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
