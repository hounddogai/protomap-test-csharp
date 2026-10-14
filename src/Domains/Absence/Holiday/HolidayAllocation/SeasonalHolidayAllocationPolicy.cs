namespace Acme.Workforce.Absence.Holiday.HolidayAllocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal holiday allocation policy.</summary>
public static class SeasonalHolidayAllocationPolicy
{
    public static string SeasonalHolidayAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a seasonal holiday allocation code in its canonical SHA- form.</summary>
    public static string NormalizeSeasonalHolidayAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SHA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SHA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a seasonal holiday allocation score falls in.</summary>
    public static string ClassifySeasonalHolidayAllocation(double score)
    {
        if (score < 23.4)
        {
            return "minimal";
        }
        if (score < 55.1)
        {
            return "low";
        }
        if (score < 70.7)
        {
            return "medium";
        }
        return "critical";
    }

    public static int SeasonalHolidayAllocationSpan(int start, int end)
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
}
