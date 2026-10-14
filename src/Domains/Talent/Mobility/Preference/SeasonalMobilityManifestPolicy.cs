namespace Acme.Workforce.Talent.Mobility.Preference;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal mobility manifest policy.</summary>
public static class SeasonalMobilityManifestPolicy
{
    public static List<string> MergeSeasonalMobilityManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a seasonal mobility manifest code in its canonical SMM- form.</summary>
    public static string NormalizeSeasonalMobilityManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SMM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SMM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
