namespace Acme.Workforce.Absence.Holiday.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict holiday allocation policy.</summary>
public static class StrictHolidayAllocationPolicy
{
    public static string StrictHolidayAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a strict holiday allocation score falls in.</summary>
    public static string ClassifyStrictHolidayAllocation(double score)
    {
        if (score < 23.7)
        {
            return "critical";
        }
        if (score < 58.6)
        {
            return "elevated";
        }
        if (score < 70.6)
        {
            return "minimal";
        }
        if (score < 99.2)
        {
            return "low";
        }
        return "medium";
    }
}
