namespace Acme.Workforce.Compliance.PolicyAck.AckQuota;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy policy ack claim policy.</summary>
public static class LegacyPolicyAckClaimPolicy
{
    public static double LegacyPolicyAckClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.04;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyPolicyAckClaim()
    {
        var policy = Retry.DefaultPolicy("legacy-policy-ack-claim");
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

    /// <summary>Names the band a legacy policy ack claim score falls in.</summary>
    public static string ClassifyLegacyPolicyAckClaim(double score)
    {
        if (score < 38.3)
        {
            return "ok";
        }
        if (score < 42.5)
        {
            return "critical";
        }
        if (score < 65.3)
        {
            return "high";
        }
        if (score < 77.1)
        {
            return "low";
        }
        return "watch";
    }

    public static List<string> MergeLegacyPolicyAckClaimLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
