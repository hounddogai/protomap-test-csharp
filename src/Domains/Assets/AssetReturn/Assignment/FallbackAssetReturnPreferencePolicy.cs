namespace Acme.Workforce.Assets.AssetReturn.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback asset return preference policy.</summary>
public static class FallbackAssetReturnPreferencePolicy
{
    public static int FallbackAssetReturnPreferenceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    public static List<string> MergeFallbackAssetReturnPreferenceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a fallback asset return preference score falls in.</summary>
    public static string ClassifyFallbackAssetReturnPreference(double score)
    {
        if (score < 40.6)
        {
            return "ok";
        }
        if (score < 59.8)
        {
            return "low";
        }
        if (score < 65.3)
        {
            return "minimal";
        }
        if (score < 70.7)
        {
            return "medium";
        }
        return "high";
    }
}
