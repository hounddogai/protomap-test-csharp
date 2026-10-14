namespace Acme.Workforce.Absence.Balance.Rollup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of balance claim policy.</summary>
public static class BalanceClaimPolicy
{
    /// <summary>Puts a balance claim code in its canonical BC- form.</summary>
    public static string NormalizeBalanceClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string BalanceClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static List<string> MergeBalanceClaimLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
