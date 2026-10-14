namespace Acme.Workforce.Assets.AssetReturn.Reservation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of asset return preference policy.</summary>
public static class AssetReturnPreferencePolicy
{
    public static List<string> MergeAssetReturnPreferenceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string AssetReturnPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a asset return preference score falls in.</summary>
    public static string ClassifyAssetReturnPreference(double score)
    {
        if (score < 7.3)
        {
            return "ok";
        }
        if (score < 54.9)
        {
            return "elevated";
        }
        return "low";
    }
}
