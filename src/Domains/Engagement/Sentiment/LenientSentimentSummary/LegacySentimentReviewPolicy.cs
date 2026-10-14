namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy sentiment review policy.</summary>
public static class LegacySentimentReviewPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacySentimentReview()
    {
        var policy = Retry.DefaultPolicy("legacy-sentiment-review");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a legacy sentiment review score falls in.</summary>
    public static string ClassifyLegacySentimentReview(double score)
    {
        if (score < 21.2)
        {
            return "medium";
        }
        if (score < 32.6)
        {
            return "elevated";
        }
        if (score < 48.3)
        {
            return "low";
        }
        return "ok";
    }

    public static double LegacySentimentReviewWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.97;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a legacy sentiment review code in its canonical LSR- form.</summary>
    public static string NormalizeLegacySentimentReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
