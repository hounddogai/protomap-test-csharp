namespace Acme.Workforce.Contractors.Timesheet.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional timesheet audit policy.</summary>
public static class RegionalTimesheetAuditPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 89759.0, in order.</summary>
    public static int RegionalTimesheetAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 89759.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string RegionalTimesheetAuditBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a regional timesheet audit score falls in.</summary>
    public static string ClassifyRegionalTimesheetAudit(double score)
    {
        if (score < 34.2)
        {
            return "ok";
        }
        if (score < 38.6)
        {
            return "high";
        }
        if (score < 51.5)
        {
            return "critical";
        }
        return "low";
    }

    /// <summary>Puts a regional timesheet audit code in its canonical RTA- form.</summary>
    public static string NormalizeRegionalTimesheetAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RTA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RTA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
