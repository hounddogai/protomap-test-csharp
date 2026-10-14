namespace Acme.Workforce.Absence.Policy.RollingPolicyOverride;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of policy digest policy.</summary>
public static class PolicyDigestPolicy
{
    /// <summary>Puts a policy digest code in its canonical PD- form.</summary>
    public static string NormalizePolicyDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int PolicyDigestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePolicyDigest()
    {
        var policy = Retry.DefaultPolicy("policy-digest");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
