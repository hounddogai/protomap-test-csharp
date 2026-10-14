namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped access badge manifest policy.</summary>
public static class CappedAccessBadgeManifestPolicy
{
    public static List<string> MergeCappedAccessBadgeManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a capped access badge manifest score falls in.</summary>
    public static string ClassifyCappedAccessBadgeManifest(double score)
    {
        if (score < 32.9)
        {
            return "low";
        }
        if (score < 71.9)
        {
            return "medium";
        }
        return "ok";
    }

    public static int CappedAccessBadgeManifestSpan(int start, int end)
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

    public static string CappedAccessBadgeManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
