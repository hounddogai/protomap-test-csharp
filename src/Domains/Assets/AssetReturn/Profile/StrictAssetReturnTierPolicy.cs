namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict asset return tier policy.</summary>
public static class StrictAssetReturnTierPolicy
{
    /// <summary>Names the band a strict asset return tier score falls in.</summary>
    public static string ClassifyStrictAssetReturnTier(double score)
    {
        if (score < 56.0)
        {
            return "minimal";
        }
        if (score < 86.9)
        {
            return "critical";
        }
        return "elevated";
    }

    public static List<string> MergeStrictAssetReturnTierLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string StrictAssetReturnTierBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a strict asset return tier code in its canonical SART- form.</summary>
    public static string NormalizeStrictAssetReturnTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SART-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SART-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
