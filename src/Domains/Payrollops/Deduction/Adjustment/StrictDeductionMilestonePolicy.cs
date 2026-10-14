namespace Acme.Workforce.Payrollops.Deduction.Adjustment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict deduction milestone policy.</summary>
public static class StrictDeductionMilestonePolicy
{
    public static double StrictDeductionMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.63;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictDeductionMilestone()
    {
        var policy = Retry.DefaultPolicy("strict-deduction-milestone");
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

    /// <summary>Puts a strict deduction milestone code in its canonical SDM- form.</summary>
    public static string NormalizeStrictDeductionMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SDM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SDM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a strict deduction milestone score falls in.</summary>
    public static string ClassifyStrictDeductionMilestone(double score)
    {
        if (score < 20.3)
        {
            return "ok";
        }
        if (score < 24.1)
        {
            return "high";
        }
        if (score < 29.9)
        {
            return "minimal";
        }
        if (score < 73.6)
        {
            return "elevated";
        }
        return "critical";
    }
}
