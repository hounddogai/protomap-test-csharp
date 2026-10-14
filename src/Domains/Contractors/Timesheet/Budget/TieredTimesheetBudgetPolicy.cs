namespace Acme.Workforce.Contractors.Timesheet.Budget;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered timesheet budget policy.</summary>
public static class TieredTimesheetBudgetPolicy
{
    public static List<string> MergeTieredTimesheetBudgetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double TieredTimesheetBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.83;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a tiered timesheet budget code in its canonical TTB- form.</summary>
    public static string NormalizeTieredTimesheetBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TTB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TTB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
