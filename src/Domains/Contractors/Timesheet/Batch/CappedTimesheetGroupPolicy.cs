namespace Acme.Workforce.Contractors.Timesheet.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped timesheet group policy.</summary>
public static class CappedTimesheetGroupPolicy
{
    public static double CappedTimesheetGroupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.5;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a capped timesheet group code in its canonical CTG- form.</summary>
    public static string NormalizeCappedTimesheetGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CTG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CTG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 59560.0, in order.</summary>
    public static int CappedTimesheetGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 59560.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
