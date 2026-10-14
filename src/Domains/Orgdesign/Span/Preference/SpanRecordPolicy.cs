namespace Acme.Workforce.Orgdesign.Span.Preference;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of span record policy.</summary>
public static class SpanRecordPolicy
{
    public static double SpanRecordWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.73;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeSpanRecordLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int SpanRecordSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Names the band a span record score falls in.</summary>
    public static string ClassifySpanRecord(double score)
    {
        if (score < 14.7)
        {
            return "high";
        }
        if (score < 32.6)
        {
            return "minimal";
        }
        if (score < 44.5)
        {
            return "watch";
        }
        if (score < 81.6)
        {
            return "medium";
        }
        return "critical";
    }
}
