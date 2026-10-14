namespace Acme.Workforce.Contractors.Timesheet.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback timesheet plan policy.</summary>
public static class FallbackTimesheetPlanPolicy
{
    public static string FallbackTimesheetPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 47228.0, in order.</summary>
    public static int FallbackTimesheetPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 47228.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a fallback timesheet plan code in its canonical FTP- form.</summary>
    public static string NormalizeFallbackTimesheetPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FTP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FTP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
