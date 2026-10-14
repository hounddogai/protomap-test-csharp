namespace Acme.Workforce.Contractors.Rate.RateNotice;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict rate notice policy.</summary>
public static class StrictRateNoticePolicy
{
    /// <summary>Names the band a strict rate notice score falls in.</summary>
    public static string ClassifyStrictRateNotice(double score)
    {
        if (score < 14.8)
        {
            return "minimal";
        }
        if (score < 50.5)
        {
            return "medium";
        }
        if (score < 56.2)
        {
            return "elevated";
        }
        return "critical";
    }

    public static List<string> MergeStrictRateNoticeLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int StrictRateNoticeSpan(int start, int end)
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

    public static double StrictRateNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.99;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
