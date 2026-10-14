namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback asset return draft policy.</summary>
public static class FallbackAssetReturnDraftPolicy
{
    public static string FallbackAssetReturnDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a fallback asset return draft score falls in.</summary>
    public static string ClassifyFallbackAssetReturnDraft(double score)
    {
        if (score < 1.7)
        {
            return "minimal";
        }
        if (score < 18.4)
        {
            return "ok";
        }
        if (score < 74.6)
        {
            return "critical";
        }
        return "elevated";
    }
}
