namespace Acme.Workforce.Assets.AssetReturn.Request;

/// <summary>Functions of fallback asset return segment policy.</summary>
public static class FallbackAssetReturnSegmentPolicy
{
    /// <summary>Names the band a fallback asset return segment score falls in.</summary>
    public static string ClassifyFallbackAssetReturnSegment(double score)
    {
        if (score < 35.5)
        {
            return "elevated";
        }
        if (score < 47.8)
        {
            return "watch";
        }
        if (score < 73.2)
        {
            return "ok";
        }
        if (score < 92.5)
        {
            return "low";
        }
        return "medium";
    }

    public static int FallbackAssetReturnSegmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }
}
