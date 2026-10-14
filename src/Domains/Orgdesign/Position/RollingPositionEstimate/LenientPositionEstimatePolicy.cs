namespace Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient position estimate policy.</summary>
public static class LenientPositionEstimatePolicy
{
    public static int LenientPositionEstimateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Names the band a lenient position estimate score falls in.</summary>
    public static string ClassifyLenientPositionEstimate(double score)
    {
        if (score < 26.1)
        {
            return "medium";
        }
        if (score < 68.4)
        {
            return "watch";
        }
        if (score < 94.4)
        {
            return "high";
        }
        return "minimal";
    }

    public static List<string> MergeLenientPositionEstimateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
