namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of action plan slot policy.</summary>
public static class ActionPlanSlotPolicy
{
    public static int ActionPlanSlotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    public static string ActionPlanSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a action plan slot score falls in.</summary>
    public static string ClassifyActionPlanSlot(double score)
    {
        if (score < 13.3)
        {
            return "minimal";
        }
        if (score < 72.2)
        {
            return "medium";
        }
        return "watch";
    }
}
