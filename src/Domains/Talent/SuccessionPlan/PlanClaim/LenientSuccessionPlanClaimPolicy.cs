namespace Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient succession plan claim policy.</summary>
public static class LenientSuccessionPlanClaimPolicy
{
    /// <summary>Puts a lenient succession plan claim code in its canonical LSPC- form.</summary>
    public static string NormalizeLenientSuccessionPlanClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSPC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSPC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientSuccessionPlanClaim()
    {
        var policy = Retry.DefaultPolicy("lenient-succession-plan-claim");
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
