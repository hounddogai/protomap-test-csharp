namespace Acme.Workforce.Compliance.Exception.Profile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pending exception manifest policy.</summary>
public static class PendingExceptionManifestPolicy
{
    public static List<string> MergePendingExceptionManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string PendingExceptionManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a pending exception manifest score falls in.</summary>
    public static string ClassifyPendingExceptionManifest(double score)
    {
        if (score < 9.1)
        {
            return "critical";
        }
        if (score < 16.1)
        {
            return "minimal";
        }
        if (score < 21.2)
        {
            return "high";
        }
        if (score < 31.9)
        {
            return "medium";
        }
        return "low";
    }
}
