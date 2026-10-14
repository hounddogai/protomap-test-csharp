namespace Acme.Workforce.Engagement.Sentiment.Adjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of sentiment preference policy.</summary>
public static class SentimentPreferencePolicy
{
    /// <summary>Names the band a sentiment preference score falls in.</summary>
    public static string ClassifySentimentPreference(double score)
    {
        if (score < 46.9)
        {
            return "medium";
        }
        if (score < 56.3)
        {
            return "high";
        }
        if (score < 82.9)
        {
            return "ok";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSentimentPreference()
    {
        var policy = Retry.DefaultPolicy("sentiment-preference");
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
