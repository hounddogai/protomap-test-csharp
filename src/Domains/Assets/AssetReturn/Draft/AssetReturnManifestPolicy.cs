namespace Acme.Workforce.Assets.AssetReturn.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of asset return manifest policy.</summary>
public static class AssetReturnManifestPolicy
{
    /// <summary>Names the band a asset return manifest score falls in.</summary>
    public static string ClassifyAssetReturnManifest(double score)
    {
        if (score < 14.6)
        {
            return "critical";
        }
        if (score < 27.5)
        {
            return "watch";
        }
        if (score < 92.0)
        {
            return "ok";
        }
        return "elevated";
    }

    public static string AssetReturnManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
