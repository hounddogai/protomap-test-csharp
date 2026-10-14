namespace Acme.Workforce.Absence.ReturnPlan.Threshold;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive return plan threshold policy.</summary>
public static class AdaptiveReturnPlanThresholdPolicy
{
    /// <summary>Puts a adaptive return plan threshold code in its canonical ARPT- form.</summary>
    public static string NormalizeAdaptiveReturnPlanThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ARPT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ARPT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string AdaptiveReturnPlanThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveReturnPlanThreshold()
    {
        var policy = Retry.DefaultPolicy("adaptive-return-plan-threshold");
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
