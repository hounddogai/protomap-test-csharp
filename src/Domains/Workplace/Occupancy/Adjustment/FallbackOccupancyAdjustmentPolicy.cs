namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback occupancy adjustment policy.</summary>
public static class FallbackOccupancyAdjustmentPolicy
{
    public static double FallbackOccupancyAdjustmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.13;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a fallback occupancy adjustment score falls in.</summary>
    public static string ClassifyFallbackOccupancyAdjustment(double score)
    {
        if (score < 15.9)
        {
            return "medium";
        }
        if (score < 54.7)
        {
            return "low";
        }
        if (score < 60.3)
        {
            return "ok";
        }
        return "high";
    }

    public static int FallbackOccupancyAdjustmentSpan(int start, int end)
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
        return span / 86400;
    }
}
