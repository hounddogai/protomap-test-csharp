namespace Acme.Workforce.Talent.SuccessionPlan.Forecast;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive succession plan policy policy.</summary>
public static class AdaptiveSuccessionPlanPolicyPolicy
{
    public static string AdaptiveSuccessionPlanPolicyBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveSuccessionPlanPolicy()
    {
        var policy = Retry.DefaultPolicy("adaptive-succession-plan-policy");
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

    /// <summary>Puts a adaptive succession plan policy code in its canonical ASPP- form.</summary>
    public static string NormalizeAdaptiveSuccessionPlanPolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ASPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ASPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a adaptive succession plan policy score falls in.</summary>
    public static string ClassifyAdaptiveSuccessionPlanPolicy(double score)
    {
        if (score < 76.8)
        {
            return "critical";
        }
        if (score < 93.0)
        {
            return "medium";
        }
        if (score < 94.4)
        {
            return "low";
        }
        if (score < 94.7)
        {
            return "ok";
        }
        return "elevated";
    }
}
