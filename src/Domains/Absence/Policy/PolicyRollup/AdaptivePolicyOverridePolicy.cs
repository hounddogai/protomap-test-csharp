namespace Acme.Workforce.Absence.Policy.PolicyRollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive policy override policy.</summary>
public static class AdaptivePolicyOverridePolicy
{
    public static double AdaptivePolicyOverrideWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.49;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a adaptive policy override score falls in.</summary>
    public static string ClassifyAdaptivePolicyOverride(double score)
    {
        if (score < 15.7)
        {
            return "elevated";
        }
        if (score < 28.3)
        {
            return "high";
        }
        return "ok";
    }
}
