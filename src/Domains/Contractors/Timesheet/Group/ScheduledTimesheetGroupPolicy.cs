namespace Acme.Workforce.Contractors.Timesheet.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled timesheet group policy.</summary>
public static class ScheduledTimesheetGroupPolicy
{
    public static int ScheduledTimesheetGroupSpan(int start, int end)
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

    public static string ScheduledTimesheetGroupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
