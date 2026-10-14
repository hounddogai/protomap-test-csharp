namespace Acme.Workforce.Engagement.Sentiment.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict sentiment quota policy.</summary>
public static class StrictSentimentQuotaPolicy
{
    public static double StrictSentimentQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.58;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int StrictSentimentQuotaSpan(int start, int end)
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

    public static string StrictSentimentQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
