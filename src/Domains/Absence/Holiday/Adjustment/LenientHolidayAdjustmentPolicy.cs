namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient holiday adjustment policy.</summary>
public static class LenientHolidayAdjustmentPolicy
{
    public static List<string> MergeLenientHolidayAdjustmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a lenient holiday adjustment code in its canonical LHA- form.</summary>
    public static string NormalizeLenientHolidayAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LHA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LHA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 54914.0, in order.</summary>
    public static int LenientHolidayAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 54914.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
