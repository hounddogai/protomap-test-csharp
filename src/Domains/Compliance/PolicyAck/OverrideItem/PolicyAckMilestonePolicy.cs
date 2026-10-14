namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of policy ack milestone policy.</summary>
public static class PolicyAckMilestonePolicy
{
    /// <summary>Puts a policy ack milestone code in its canonical PAM- form.</summary>
    public static string NormalizePolicyAckMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePolicyAckMilestone()
    {
        var policy = Retry.DefaultPolicy("policy-ack-milestone");
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
