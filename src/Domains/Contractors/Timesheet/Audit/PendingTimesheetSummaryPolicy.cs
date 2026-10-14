namespace Acme.Workforce.Contractors.Timesheet.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending timesheet summary policy.</summary>
public static class PendingTimesheetSummaryPolicy
{
    /// <summary>Names the band a pending timesheet summary score falls in.</summary>
    public static string ClassifyPendingTimesheetSummary(double score)
    {
        if (score < 26.1)
        {
            return "elevated";
        }
        if (score < 46.6)
        {
            return "watch";
        }
        if (score < 82.7)
        {
            return "critical";
        }
        return "low";
    }

    public static double PendingTimesheetSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.77;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string PendingTimesheetSummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
