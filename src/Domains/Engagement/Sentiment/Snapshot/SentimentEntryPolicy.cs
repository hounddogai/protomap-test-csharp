namespace Acme.Workforce.Engagement.Sentiment.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of sentiment entry policy.</summary>
public static class SentimentEntryPolicy
{
    public static double SentimentEntryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.89;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a sentiment entry code in its canonical SE- form.</summary>
    public static string NormalizeSentimentEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string SentimentEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
