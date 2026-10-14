namespace Acme.Workforce.Engagement.Sentiment.SentimentVariance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual sentiment variance policy.</summary>
public static class ManualSentimentVariancePolicy
{
    public static string ManualSentimentVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a manual sentiment variance code in its canonical MSV- form.</summary>
    public static string NormalizeManualSentimentVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MSV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MSV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int ManualSentimentVarianceSpan(int start, int end)
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
        return span / 3600;
    }
}
