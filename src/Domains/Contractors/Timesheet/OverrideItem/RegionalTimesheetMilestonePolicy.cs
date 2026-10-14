namespace Acme.Workforce.Contractors.Timesheet.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional timesheet milestone policy.</summary>
public static class RegionalTimesheetMilestonePolicy
{
    public static string RegionalTimesheetMilestoneBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double RegionalTimesheetMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.55;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
