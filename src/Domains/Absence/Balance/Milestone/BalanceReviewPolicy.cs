namespace Acme.Workforce.Absence.Balance.Milestone;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of balance review policy.</summary>
public static class BalanceReviewPolicy
{
    public static double BalanceReviewWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.45;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeBalanceReviewLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a balance review code in its canonical BR- form.</summary>
    public static string NormalizeBalanceReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
