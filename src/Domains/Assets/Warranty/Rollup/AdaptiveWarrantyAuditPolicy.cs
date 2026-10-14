namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive warranty audit policy.</summary>
public static class AdaptiveWarrantyAuditPolicy
{
    public static List<string> MergeAdaptiveWarrantyAuditLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a adaptive warranty audit code in its canonical AWA- form.</summary>
    public static string NormalizeAdaptiveWarrantyAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AWA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AWA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
