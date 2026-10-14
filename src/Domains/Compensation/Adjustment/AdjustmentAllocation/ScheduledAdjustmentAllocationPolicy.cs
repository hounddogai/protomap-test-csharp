namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAllocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled adjustment allocation policy.</summary>
public static class ScheduledAdjustmentAllocationPolicy
{
    /// <summary>Puts a scheduled adjustment allocation code in its canonical SAA- form.</summary>
    public static string NormalizeScheduledAdjustmentAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SAA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SAA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a scheduled adjustment allocation score falls in.</summary>
    public static string ClassifyScheduledAdjustmentAllocation(double score)
    {
        if (score < 7.6)
        {
            return "elevated";
        }
        if (score < 31.4)
        {
            return "minimal";
        }
        if (score < 97.0)
        {
            return "low";
        }
        return "ok";
    }

    public static int ScheduledAdjustmentAllocationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    public static string ScheduledAdjustmentAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
