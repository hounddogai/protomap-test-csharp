namespace Acme.Workforce.Contractors.Timesheet.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of timesheet metric policy.</summary>
public static class TimesheetMetricPolicy
{
    /// <summary>Names the band a timesheet metric score falls in.</summary>
    public static string ClassifyTimesheetMetric(double score)
    {
        if (score < 11.5)
        {
            return "ok";
        }
        if (score < 90.7)
        {
            return "elevated";
        }
        return "high";
    }

    public static double TimesheetMetricWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.8;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string TimesheetMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
