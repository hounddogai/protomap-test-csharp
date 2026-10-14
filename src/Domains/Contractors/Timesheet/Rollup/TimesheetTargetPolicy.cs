namespace Acme.Workforce.Contractors.Timesheet.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of timesheet target policy.</summary>
public static class TimesheetTargetPolicy
{
    /// <summary>Names the band a timesheet target score falls in.</summary>
    public static string ClassifyTimesheetTarget(double score)
    {
        if (score < 29.9)
        {
            return "critical";
        }
        if (score < 33.0)
        {
            return "low";
        }
        if (score < 33.4)
        {
            return "watch";
        }
        return "ok";
    }

    public static double TimesheetTargetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.54;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
