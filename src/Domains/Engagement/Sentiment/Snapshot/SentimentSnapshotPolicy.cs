namespace Acme.Workforce.Engagement.Sentiment.Snapshot;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of sentiment snapshot policy.</summary>
public static class SentimentSnapshotPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSentimentSnapshot()
    {
        var policy = Retry.DefaultPolicy("sentiment-snapshot");
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

    /// <summary>Names the band a sentiment snapshot score falls in.</summary>
    public static string ClassifySentimentSnapshot(double score)
    {
        if (score < 15.2)
        {
            return "low";
        }
        if (score < 39.3)
        {
            return "elevated";
        }
        if (score < 69.8)
        {
            return "medium";
        }
        return "minimal";
    }
}
