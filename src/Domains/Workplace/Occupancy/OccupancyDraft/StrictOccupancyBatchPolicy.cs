namespace Acme.Workforce.Workplace.Occupancy.OccupancyDraft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict occupancy batch policy.</summary>
public static class StrictOccupancyBatchPolicy
{
    public static double StrictOccupancyBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.16;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a strict occupancy batch score falls in.</summary>
    public static string ClassifyStrictOccupancyBatch(double score)
    {
        if (score < 5.9)
        {
            return "critical";
        }
        if (score < 74.8)
        {
            return "watch";
        }
        return "low";
    }

    public static int StrictOccupancyBatchSpan(int start, int end)
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
        return span / 60;
    }
}
