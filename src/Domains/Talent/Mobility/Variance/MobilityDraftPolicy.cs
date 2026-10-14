namespace Acme.Workforce.Talent.Mobility.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of mobility draft policy.</summary>
public static class MobilityDraftPolicy
{
    public static int MobilityDraftSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Names the band a mobility draft score falls in.</summary>
    public static string ClassifyMobilityDraft(double score)
    {
        if (score < 39.2)
        {
            return "medium";
        }
        if (score < 61.0)
        {
            return "low";
        }
        if (score < 64.1)
        {
            return "minimal";
        }
        if (score < 94.6)
        {
            return "high";
        }
        return "watch";
    }

    public static string MobilityDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
