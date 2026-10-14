namespace Acme.Workforce.Assets.Refresh.Preference;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of refresh digest policy.</summary>
public static class RefreshDigestPolicy
{
    public static string RefreshDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a refresh digest score falls in.</summary>
    public static string ClassifyRefreshDigest(double score)
    {
        if (score < 16.9)
        {
            return "medium";
        }
        if (score < 59.2)
        {
            return "watch";
        }
        if (score < 97.4)
        {
            return "elevated";
        }
        return "critical";
    }

    public static List<string> MergeRefreshDigestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
