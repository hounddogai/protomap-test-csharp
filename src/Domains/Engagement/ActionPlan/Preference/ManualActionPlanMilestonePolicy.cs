namespace Acme.Workforce.Engagement.ActionPlan.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual action plan milestone policy.</summary>
public static class ManualActionPlanMilestonePolicy
{
    /// <summary>Puts a manual action plan milestone code in its canonical MAPM- form.</summary>
    public static string NormalizeManualActionPlanMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MAPM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MAPM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int ManualActionPlanMilestoneSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }

    public static string ManualActionPlanMilestoneBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static double ManualActionPlanMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.36;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
