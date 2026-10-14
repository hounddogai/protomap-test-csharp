namespace Acme.Workforce.Talent.SuccessionPlan.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of succession plan preference policy.</summary>
public static class SuccessionPlanPreferencePolicy
{
    /// <summary>Names the band a succession plan preference score falls in.</summary>
    public static string ClassifySuccessionPlanPreference(double score)
    {
        if (score < 32.8)
        {
            return "low";
        }
        if (score < 76.3)
        {
            return "critical";
        }
        if (score < 85.7)
        {
            return "elevated";
        }
        return "minimal";
    }

    public static double SuccessionPlanPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.43;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
