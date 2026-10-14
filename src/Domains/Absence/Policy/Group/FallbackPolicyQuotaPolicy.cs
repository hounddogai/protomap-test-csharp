namespace Acme.Workforce.Absence.Policy.Group;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback policy quota policy.</summary>
public static class FallbackPolicyQuotaPolicy
{
    public static int FallbackPolicyQuotaSpan(int start, int end)
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
    public static int ProbeFallbackPolicyQuota()
    {
        var policy = Retry.DefaultPolicy("fallback-policy-quota");
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

    /// <summary>Puts a fallback policy quota code in its canonical FPQ- form.</summary>
    public static string NormalizeFallbackPolicyQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FPQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FPQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string FallbackPolicyQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
