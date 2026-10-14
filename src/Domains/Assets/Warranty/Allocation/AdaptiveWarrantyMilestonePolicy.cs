namespace Acme.Workforce.Assets.Warranty.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive warranty milestone policy.</summary>
public static class AdaptiveWarrantyMilestonePolicy
{
    public static string AdaptiveWarrantyMilestoneBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a adaptive warranty milestone score falls in.</summary>
    public static string ClassifyAdaptiveWarrantyMilestone(double score)
    {
        if (score < 22.2)
        {
            return "elevated";
        }
        if (score < 24.0)
        {
            return "ok";
        }
        if (score < 71.5)
        {
            return "watch";
        }
        return "high";
    }
}
