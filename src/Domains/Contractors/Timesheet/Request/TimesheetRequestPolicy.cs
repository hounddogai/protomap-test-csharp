namespace Acme.Workforce.Contractors.Timesheet.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of timesheet request policy.</summary>
public static class TimesheetRequestPolicy
{
    public static double TimesheetRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.85;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a timesheet request score falls in.</summary>
    public static string ClassifyTimesheetRequest(double score)
    {
        if (score < 19.4)
        {
            return "ok";
        }
        if (score < 50.0)
        {
            return "elevated";
        }
        if (score < 51.2)
        {
            return "watch";
        }
        if (score < 58.7)
        {
            return "high";
        }
        return "medium";
    }

    public static string TimesheetRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
