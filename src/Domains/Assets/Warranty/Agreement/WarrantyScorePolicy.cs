namespace Acme.Workforce.Assets.Warranty.Agreement;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of warranty score policy.</summary>
public static class WarrantyScorePolicy
{
    /// <summary>Puts a warranty score code in its canonical WS- form.</summary>
    public static string NormalizeWarrantyScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeWarrantyScoreLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
