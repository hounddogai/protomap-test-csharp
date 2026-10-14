namespace Acme.Workforce.Absence.Balance.Request;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of balance request policy.</summary>
public static class BalanceRequestPolicy
{
    public static List<string> MergeBalanceRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a balance request code in its canonical BR- form.</summary>
    public static string NormalizeBalanceRequestCode(string code)
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

    public static int BalanceRequestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }
}
