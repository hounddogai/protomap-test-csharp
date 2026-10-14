namespace Acme.Workforce.Engagement.Sentiment.Baseline;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of sentiment baseline policy.</summary>
public static class SentimentBaselinePolicy
{
    public static double SentimentBaselineWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.52;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a sentiment baseline score falls in.</summary>
    public static string ClassifySentimentBaseline(double score)
    {
        if (score < 12.0)
        {
            return "ok";
        }
        if (score < 36.8)
        {
            return "critical";
        }
        if (score < 70.9)
        {
            return "elevated";
        }
        return "minimal";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSentimentBaseline()
    {
        var policy = Retry.DefaultPolicy("sentiment-baseline");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
