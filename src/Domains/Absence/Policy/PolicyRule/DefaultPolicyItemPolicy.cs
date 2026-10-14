namespace Acme.Workforce.Absence.Policy.PolicyRule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default policy item policy.</summary>
public static class DefaultPolicyItemPolicy
{
    public static double DefaultPolicyItemWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.76;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a default policy item score falls in.</summary>
    public static string ClassifyDefaultPolicyItem(double score)
    {
        if (score < 18.6)
        {
            return "medium";
        }
        if (score < 37.6)
        {
            return "minimal";
        }
        if (score < 66.8)
        {
            return "ok";
        }
        return "critical";
    }
}
