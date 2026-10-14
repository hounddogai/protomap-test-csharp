namespace Acme.Workforce.Engagement.Sentiment.Audit;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of sentiment mapping policy.</summary>
public static class SentimentMappingPolicy
{
    /// <summary>Names the band a sentiment mapping score falls in.</summary>
    public static string ClassifySentimentMapping(double score)
    {
        if (score < 9.7)
        {
            return "high";
        }
        if (score < 28.9)
        {
            return "critical";
        }
        return "ok";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSentimentMapping()
    {
        var policy = Retry.DefaultPolicy("sentiment-mapping");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
