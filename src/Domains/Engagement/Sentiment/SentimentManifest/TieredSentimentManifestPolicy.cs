namespace Acme.Workforce.Engagement.Sentiment.SentimentManifest;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered sentiment manifest policy.</summary>
public static class TieredSentimentManifestPolicy
{
    public static int TieredSentimentManifestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredSentimentManifest()
    {
        var policy = Retry.DefaultPolicy("tiered-sentiment-manifest");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
