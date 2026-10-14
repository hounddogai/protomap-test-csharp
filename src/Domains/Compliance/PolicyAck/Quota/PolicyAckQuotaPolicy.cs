namespace Acme.Workforce.Compliance.PolicyAck.Quota;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of policy ack quota policy.</summary>
public static class PolicyAckQuotaPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePolicyAckQuota()
    {
        var policy = Retry.DefaultPolicy("policy-ack-quota");
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

    public static int PolicyAckQuotaSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Puts a policy ack quota code in its canonical PAQ- form.</summary>
    public static string NormalizePolicyAckQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
