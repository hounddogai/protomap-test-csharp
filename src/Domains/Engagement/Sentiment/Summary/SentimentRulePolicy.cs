namespace Acme.Workforce.Engagement.Sentiment.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of sentiment rule policy.</summary>
public static class SentimentRulePolicy
{
    public static double SentimentRuleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.84;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a sentiment rule code in its canonical SR- form.</summary>
    public static string NormalizeSentimentRuleCode(string code)
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
