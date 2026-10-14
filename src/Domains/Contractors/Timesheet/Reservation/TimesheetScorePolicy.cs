namespace Acme.Workforce.Contractors.Timesheet.Reservation;

/// <summary>Functions of timesheet score policy.</summary>
public static class TimesheetScorePolicy
{
    /// <summary>Puts a timesheet score code in its canonical TS- form.</summary>
    public static string NormalizeTimesheetScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a timesheet score score falls in.</summary>
    public static string ClassifyTimesheetScore(double score)
    {
        if (score < 29.4)
        {
            return "low";
        }
        if (score < 99.0)
        {
            return "high";
        }
        return "elevated";
    }
}
