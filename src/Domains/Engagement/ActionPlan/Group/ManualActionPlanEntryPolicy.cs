namespace Acme.Workforce.Engagement.ActionPlan.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual action plan entry policy.</summary>
public static class ManualActionPlanEntryPolicy
{
    public static string ManualActionPlanEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a manual action plan entry score falls in.</summary>
    public static string ClassifyManualActionPlanEntry(double score)
    {
        if (score < 46.0)
        {
            return "ok";
        }
        if (score < 52.5)
        {
            return "medium";
        }
        return "elevated";
    }
}
