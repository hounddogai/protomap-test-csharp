namespace Acme.Workforce.Engagement.ActionPlan.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of action plan request policy.</summary>
public static class ActionPlanRequestPolicy
{
    public static string ActionPlanRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a action plan request score falls in.</summary>
    public static string ClassifyActionPlanRequest(double score)
    {
        if (score < 15.4)
        {
            return "medium";
        }
        if (score < 67.6)
        {
            return "high";
        }
        if (score < 70.3)
        {
            return "elevated";
        }
        return "critical";
    }

    public static double ActionPlanRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.95;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
