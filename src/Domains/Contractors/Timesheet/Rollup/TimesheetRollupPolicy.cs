namespace Acme.Workforce.Contractors.Timesheet.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of timesheet rollup policy.</summary>
public static class TimesheetRollupPolicy
{
    /// <summary>Puts a timesheet rollup code in its canonical TR- form.</summary>
    public static string NormalizeTimesheetRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string TimesheetRollupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
