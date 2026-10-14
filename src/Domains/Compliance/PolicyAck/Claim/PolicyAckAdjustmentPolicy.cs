namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of policy ack adjustment policy.</summary>
public static class PolicyAckAdjustmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePolicyAckAdjustment()
    {
        var policy = Retry.DefaultPolicy("policy-ack-adjustment");
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

    public static List<string> MergePolicyAckAdjustmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a policy ack adjustment code in its canonical PAA- form.</summary>
    public static string NormalizePolicyAckAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
