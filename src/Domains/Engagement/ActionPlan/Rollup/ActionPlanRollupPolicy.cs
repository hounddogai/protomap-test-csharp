namespace Acme.Workforce.Engagement.ActionPlan.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of action plan rollup policy.</summary>
public static class ActionPlanRollupPolicy
{
    /// <summary>Names the band a action plan rollup score falls in.</summary>
    public static string ClassifyActionPlanRollup(double score)
    {
        if (score < 7.8)
        {
            return "high";
        }
        if (score < 91.6)
        {
            return "medium";
        }
        return "elevated";
    }

    public static double ActionPlanRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.75;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
