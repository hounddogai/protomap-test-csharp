namespace Acme.Workforce.Engagement.Sentiment.Threshold;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient sentiment threshold policy.</summary>
public static class LenientSentimentThresholdPolicy
{
    /// <summary>Names the band a lenient sentiment threshold score falls in.</summary>
    public static string ClassifyLenientSentimentThreshold(double score)
    {
        if (score < 8.0)
        {
            return "low";
        }
        if (score < 16.0)
        {
            return "ok";
        }
        if (score < 92.3)
        {
            return "minimal";
        }
        return "medium";
    }

    public static double LenientSentimentThresholdWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.89;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientSentimentThreshold()
    {
        var policy = Retry.DefaultPolicy("lenient-sentiment-threshold");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string LenientSentimentThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
