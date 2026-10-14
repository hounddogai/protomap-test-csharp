namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient mobility request policy.</summary>
public static class LenientMobilityRequestPolicy
{
    public static int LenientMobilityRequestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Names the band a lenient mobility request score falls in.</summary>
    public static string ClassifyLenientMobilityRequest(double score)
    {
        if (score < 11.4)
        {
            return "medium";
        }
        if (score < 14.9)
        {
            return "low";
        }
        if (score < 42.7)
        {
            return "high";
        }
        if (score < 43.8)
        {
            return "minimal";
        }
        return "watch";
    }

    public static List<string> MergeLenientMobilityRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string LenientMobilityRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
