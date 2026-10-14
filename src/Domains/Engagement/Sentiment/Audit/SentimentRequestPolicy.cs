namespace Acme.Workforce.Engagement.Sentiment.Audit;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of sentiment request policy.</summary>
public static class SentimentRequestPolicy
{
    /// <summary>Names the band a sentiment request score falls in.</summary>
    public static string ClassifySentimentRequest(double score)
    {
        if (score < 3.9)
        {
            return "elevated";
        }
        if (score < 22.7)
        {
            return "minimal";
        }
        if (score < 23.2)
        {
            return "high";
        }
        if (score < 80.1)
        {
            return "watch";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSentimentRequest()
    {
        var policy = Retry.DefaultPolicy("sentiment-request");
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

    /// <summary>Puts a sentiment request code in its canonical SR- form.</summary>
    public static string NormalizeSentimentRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int SentimentRequestSpan(int start, int end)
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
