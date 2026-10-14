namespace Acme.Workforce.Compliance.PolicyAck.Entry;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted policy ack bundle policy.</summary>
public static class WeightedPolicyAckBundlePolicy
{
    /// <summary>Names the band a weighted policy ack bundle score falls in.</summary>
    public static string ClassifyWeightedPolicyAckBundle(double score)
    {
        if (score < 34.9)
        {
            return "medium";
        }
        if (score < 39.8)
        {
            return "ok";
        }
        if (score < 56.2)
        {
            return "high";
        }
        if (score < 90.4)
        {
            return "elevated";
        }
        return "minimal";
    }

    public static List<string> MergeWeightedPolicyAckBundleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int WeightedPolicyAckBundleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedPolicyAckBundle()
    {
        var policy = Retry.DefaultPolicy("weighted-policy-ack-bundle");
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
