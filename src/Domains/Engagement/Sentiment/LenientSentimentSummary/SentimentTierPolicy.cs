namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of sentiment tier policy.</summary>
public static class SentimentTierPolicy
{
    public static double SentimentTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.17;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int SentimentTierSpan(int start, int end)
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
        return span / 86400;
    }
}
