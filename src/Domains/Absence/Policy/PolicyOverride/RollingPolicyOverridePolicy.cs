namespace Acme.Workforce.Absence.Policy.PolicyOverride;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling policy override policy.</summary>
public static class RollingPolicyOverridePolicy
{
    public static int RollingPolicyOverrideSpan(int start, int end)
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
        return span / 86400;
    }

    public static double RollingPolicyOverrideWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.33;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a rolling policy override score falls in.</summary>
    public static string ClassifyRollingPolicyOverride(double score)
    {
        if (score < 6.4)
        {
            return "low";
        }
        if (score < 26.9)
        {
            return "minimal";
        }
        return "high";
    }
}
