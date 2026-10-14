namespace Acme.Workforce.Engagement.Sentiment.SentimentVariance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of sentiment agreement policy.</summary>
public static class SentimentAgreementPolicy
{
    /// <summary>Puts a sentiment agreement code in its canonical SA- form.</summary>
    public static string NormalizeSentimentAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double SentimentAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.1;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
