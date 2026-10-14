namespace Acme.Workforce.Compliance.PolicyAck.AckQuota;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary policy ack override policy.</summary>
public static class PrimaryPolicyAckOverridePolicy
{
    /// <summary>Puts a primary policy ack override code in its canonical PPAO- form.</summary>
    public static string NormalizePrimaryPolicyAckOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PPAO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PPAO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryPolicyAckOverride()
    {
        var policy = Retry.DefaultPolicy("primary-policy-ack-override");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
