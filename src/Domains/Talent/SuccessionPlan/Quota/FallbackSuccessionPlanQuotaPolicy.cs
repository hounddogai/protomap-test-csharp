namespace Acme.Workforce.Talent.SuccessionPlan.Quota;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback succession plan quota policy.</summary>
public static class FallbackSuccessionPlanQuotaPolicy
{
    /// <summary>Puts a fallback succession plan quota code in its canonical FSPQ- form.</summary>
    public static string NormalizeFallbackSuccessionPlanQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FSPQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FSPQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string FallbackSuccessionPlanQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static double FallbackSuccessionPlanQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.3;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackSuccessionPlanQuota()
    {
        var policy = Retry.DefaultPolicy("fallback-succession-plan-quota");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
