namespace Acme.Workforce.Contractors.Timesheet.Policy;

/// <summary>Functions of timesheet draft policy.</summary>
public static class TimesheetDraftPolicy
{
    /// <summary>Puts a timesheet draft code in its canonical TD- form.</summary>
    public static string NormalizeTimesheetDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a timesheet draft score falls in.</summary>
    public static string ClassifyTimesheetDraft(double score)
    {
        if (score < 55.7)
        {
            return "minimal";
        }
        if (score < 66.7)
        {
            return "low";
        }
        return "medium";
    }
}
