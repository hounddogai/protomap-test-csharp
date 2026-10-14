namespace Acme.Workforce.Contractors.Timesheet.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of timesheet assignment policy.</summary>
public static class TimesheetAssignmentPolicy
{
    /// <summary>Puts a timesheet assignment code in its canonical TA- form.</summary>
    public static string NormalizeTimesheetAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a timesheet assignment score falls in.</summary>
    public static string ClassifyTimesheetAssignment(double score)
    {
        if (score < 7.2)
        {
            return "elevated";
        }
        if (score < 7.4)
        {
            return "critical";
        }
        if (score < 44.1)
        {
            return "low";
        }
        if (score < 79.9)
        {
            return "minimal";
        }
        return "ok";
    }

    public static string TimesheetAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
