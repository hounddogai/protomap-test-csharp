namespace Acme.Workforce.Assets.Warranty.Forecast;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of warranty request policy.</summary>
public static class WarrantyRequestPolicy
{
    /// <summary>Puts a warranty request code in its canonical WR- form.</summary>
    public static string NormalizeWarrantyRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeWarrantyRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
