namespace Acme.Workforce.Engagement.Sentiment.Adjustment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of sentiment checkpoint policy.</summary>
public static class SentimentCheckpointPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSentimentCheckpoint()
    {
        var policy = Retry.DefaultPolicy("sentiment-checkpoint");
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

    public static string SentimentCheckpointBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static int SentimentCheckpointSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Puts a sentiment checkpoint code in its canonical SC- form.</summary>
    public static string NormalizeSentimentCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
