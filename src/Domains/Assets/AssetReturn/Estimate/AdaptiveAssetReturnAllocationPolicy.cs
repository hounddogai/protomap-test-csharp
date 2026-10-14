namespace Acme.Workforce.Assets.AssetReturn.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive asset return allocation policy.</summary>
public static class AdaptiveAssetReturnAllocationPolicy
{
    public static string AdaptiveAssetReturnAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double AdaptiveAssetReturnAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.6;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a adaptive asset return allocation score falls in.</summary>
    public static string ClassifyAdaptiveAssetReturnAllocation(double score)
    {
        if (score < 35.7)
        {
            return "medium";
        }
        if (score < 55.0)
        {
            return "minimal";
        }
        if (score < 62.5)
        {
            return "watch";
        }
        return "ok";
    }
}
