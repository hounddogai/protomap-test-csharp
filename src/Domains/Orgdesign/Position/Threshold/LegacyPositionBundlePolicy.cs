namespace Acme.Workforce.Orgdesign.Position.Threshold;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy position bundle policy.</summary>
public static class LegacyPositionBundlePolicy
{
    public static List<string> MergeLegacyPositionBundleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a legacy position bundle code in its canonical LPB- form.</summary>
    public static string NormalizeLegacyPositionBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LPB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LPB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
