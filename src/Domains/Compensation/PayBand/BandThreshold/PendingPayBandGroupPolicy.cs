namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pending pay band group policy.</summary>
public static class PendingPayBandGroupPolicy
{
    public static string PendingPayBandGroupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static List<string> MergePendingPayBandGroupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int PendingPayBandGroupSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Names the band a pending pay band group score falls in.</summary>
    public static string ClassifyPendingPayBandGroup(double score)
    {
        if (score < 44.7)
        {
            return "watch";
        }
        if (score < 60.6)
        {
            return "low";
        }
        return "elevated";
    }
}
