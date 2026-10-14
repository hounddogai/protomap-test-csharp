namespace Acme.Workforce.Contractors.Engagement.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of engagement rollup policy.</summary>
public static class EngagementRollupPolicy
{
    public static double EngagementRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.36;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int EngagementRollupSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }
}
