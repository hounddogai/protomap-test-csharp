namespace Acme.Workforce.Absence.Holiday.Request;

/// <summary>Functions of holiday request policy.</summary>
public static class HolidayRequestPolicy
{
    /// <summary>Names the band a holiday request score falls in.</summary>
    public static string ClassifyHolidayRequest(double score)
    {
        if (score < 50.0)
        {
            return "minimal";
        }
        if (score < 61.3)
        {
            return "ok";
        }
        if (score < 89.6)
        {
            return "elevated";
        }
        return "medium";
    }

    public static int HolidayRequestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }
}
