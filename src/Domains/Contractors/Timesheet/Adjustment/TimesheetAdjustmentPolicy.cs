namespace Acme.Workforce.Contractors.Timesheet.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of timesheet adjustment policy.</summary>
public static class TimesheetAdjustmentPolicy
{
    public static double TimesheetAdjustmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.59;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string TimesheetAdjustmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
