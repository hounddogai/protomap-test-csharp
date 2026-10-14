namespace Acme.Workforce.Contractors.Timesheet.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of timesheet tier policy.</summary>
public static class TimesheetTierPolicy
{
    /// <summary>Puts a timesheet tier code in its canonical TT- form.</summary>
    public static string NormalizeTimesheetTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a timesheet tier score falls in.</summary>
    public static string ClassifyTimesheetTier(double score)
    {
        if (score < 6.6)
        {
            return "ok";
        }
        if (score < 30.2)
        {
            return "minimal";
        }
        if (score < 75.8)
        {
            return "high";
        }
        return "critical";
    }

    public static double TimesheetTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.35;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 20596.0, in order.</summary>
    public static int TimesheetTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 20596.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
