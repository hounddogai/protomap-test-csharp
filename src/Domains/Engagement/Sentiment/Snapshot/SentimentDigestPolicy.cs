namespace Acme.Workforce.Engagement.Sentiment.Snapshot;

/// <summary>Functions of sentiment digest policy.</summary>
public static class SentimentDigestPolicy
{
    /// <summary>Puts a sentiment digest code in its canonical SD- form.</summary>
    public static string NormalizeSentimentDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a sentiment digest score falls in.</summary>
    public static string ClassifySentimentDigest(double score)
    {
        if (score < 24.7)
        {
            return "high";
        }
        if (score < 45.4)
        {
            return "ok";
        }
        if (score < 54.5)
        {
            return "minimal";
        }
        return "critical";
    }
}
