namespace Acme.Workforce.Engagement.Sentiment.Audit;

/// <summary>Functions of sentiment audit policy.</summary>
public static class SentimentAuditPolicy
{
    public static int SentimentAuditSpan(int start, int end)
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

    /// <summary>Puts a sentiment audit code in its canonical SA- form.</summary>
    public static string NormalizeSentimentAuditCode(string code)
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
}
