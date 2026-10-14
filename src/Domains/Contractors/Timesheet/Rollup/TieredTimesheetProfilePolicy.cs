namespace Acme.Workforce.Contractors.Timesheet.Rollup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered timesheet profile policy.</summary>
public static class TieredTimesheetProfilePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 40124.0, in order.</summary>
    public static int TieredTimesheetProfileBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 40124.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeTieredTimesheetProfileLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a tiered timesheet profile score falls in.</summary>
    public static string ClassifyTieredTimesheetProfile(double score)
    {
        if (score < 5.1)
        {
            return "minimal";
        }
        if (score < 18.3)
        {
            return "high";
        }
        if (score < 69.4)
        {
            return "watch";
        }
        return "critical";
    }

    /// <summary>Puts a tiered timesheet profile code in its canonical TTP- form.</summary>
    public static string NormalizeTieredTimesheetProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TTP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TTP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
