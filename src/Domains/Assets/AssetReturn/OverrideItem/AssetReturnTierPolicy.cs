namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of asset return tier policy.</summary>
public static class AssetReturnTierPolicy
{
    public static double AssetReturnTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.05;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a asset return tier code in its canonical ART- form.</summary>
    public static string NormalizeAssetReturnTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ART-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ART-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a asset return tier score falls in.</summary>
    public static string ClassifyAssetReturnTier(double score)
    {
        if (score < 87.4)
        {
            return "elevated";
        }
        if (score < 90.0)
        {
            return "low";
        }
        return "medium";
    }
}
