namespace Acme.Workforce.Engagement.ActionPlan.PlanPreference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of action plan policy policy.</summary>
public static class ActionPlanPolicyPolicy
{
    public static string ActionPlanPolicyBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double ActionPlanPolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.52;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a action plan policy score falls in.</summary>
    public static string ClassifyActionPlanPolicy(double score)
    {
        if (score < 45.2)
        {
            return "ok";
        }
        if (score < 58.5)
        {
            return "minimal";
        }
        return "medium";
    }
}
