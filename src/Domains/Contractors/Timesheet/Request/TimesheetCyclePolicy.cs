namespace Acme.Workforce.Contractors.Timesheet.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of timesheet cycle policy.</summary>
public static class TimesheetCyclePolicy
{
    public static double TimesheetCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.5;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a timesheet cycle code in its canonical TC- form.</summary>
    public static string NormalizeTimesheetCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
