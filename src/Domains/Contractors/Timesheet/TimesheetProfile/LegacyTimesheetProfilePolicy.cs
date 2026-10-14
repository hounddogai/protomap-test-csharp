namespace Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

/// <summary>Functions of legacy timesheet profile policy.</summary>
public static class LegacyTimesheetProfilePolicy
{
    /// <summary>Puts a legacy timesheet profile code in its canonical LTP- form.</summary>
    public static string NormalizeLegacyTimesheetProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LTP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LTP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a legacy timesheet profile score falls in.</summary>
    public static string ClassifyLegacyTimesheetProfile(double score)
    {
        if (score < 57.9)
        {
            return "watch";
        }
        if (score < 68.4)
        {
            return "elevated";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 21301.0, in order.</summary>
    public static int LegacyTimesheetProfileBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 21301.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int LegacyTimesheetProfileSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }
}
