namespace Acme.Workforce.Assets.AssetReturn.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual asset return tier policy.</summary>
public static class ManualAssetReturnTierPolicy
{
    public static double ManualAssetReturnTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.46;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a manual asset return tier score falls in.</summary>
    public static string ClassifyManualAssetReturnTier(double score)
    {
        if (score < 81.1)
        {
            return "critical";
        }
        if (score < 99.3)
        {
            return "ok";
        }
        return "high";
    }
}
