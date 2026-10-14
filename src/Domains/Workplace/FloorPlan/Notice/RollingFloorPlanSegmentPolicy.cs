namespace Acme.Workforce.Workplace.FloorPlan.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling floor plan segment policy.</summary>
public static class RollingFloorPlanSegmentPolicy
{
    public static int RollingFloorPlanSegmentSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Names the band a rolling floor plan segment score falls in.</summary>
    public static string ClassifyRollingFloorPlanSegment(double score)
    {
        if (score < 30.7)
        {
            return "medium";
        }
        if (score < 46.9)
        {
            return "minimal";
        }
        if (score < 82.9)
        {
            return "critical";
        }
        if (score < 88.0)
        {
            return "watch";
        }
        return "low";
    }

    public static string RollingFloorPlanSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static double RollingFloorPlanSegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.42;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
