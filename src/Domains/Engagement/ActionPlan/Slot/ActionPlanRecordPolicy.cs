namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of action plan record policy.</summary>
public static class ActionPlanRecordPolicy
{
    /// <summary>Names the band a action plan record score falls in.</summary>
    public static string ClassifyActionPlanRecord(double score)
    {
        if (score < 22.3)
        {
            return "low";
        }
        if (score < 49.5)
        {
            return "ok";
        }
        return "watch";
    }

    public static string ActionPlanRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
