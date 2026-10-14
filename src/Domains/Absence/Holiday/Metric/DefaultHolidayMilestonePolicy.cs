namespace Acme.Workforce.Absence.Holiday.Metric;

/// <summary>Functions of default holiday milestone policy.</summary>
public static class DefaultHolidayMilestonePolicy
{
    public static int DefaultHolidayMilestoneSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Names the band a default holiday milestone score falls in.</summary>
    public static string ClassifyDefaultHolidayMilestone(double score)
    {
        if (score < 44.9)
        {
            return "low";
        }
        if (score < 66.9)
        {
            return "ok";
        }
        return "elevated";
    }
}
