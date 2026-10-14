namespace Acme.Workforce.Engagement.Sentiment.Reservation;

/// <summary>Functions of sentiment review policy.</summary>
public static class SentimentReviewPolicy
{
    /// <summary>Names the band a sentiment review score falls in.</summary>
    public static string ClassifySentimentReview(double score)
    {
        if (score < 3.5)
        {
            return "minimal";
        }
        if (score < 25.0)
        {
            return "critical";
        }
        if (score < 46.4)
        {
            return "ok";
        }
        if (score < 70.4)
        {
            return "high";
        }
        return "watch";
    }

    /// <summary>Puts a sentiment review code in its canonical SR- form.</summary>
    public static string NormalizeSentimentReviewCode(string code)
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
}
