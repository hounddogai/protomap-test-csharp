namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of desk baseline policy.</summary>
public static class DeskBaselinePolicy
{
    public static double DeskBaselineWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.14;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeDeskBaselineLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a desk baseline code in its canonical DB- form.</summary>
    public static string NormalizeDeskBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
