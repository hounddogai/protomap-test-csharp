namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled sentiment ledger policy.</summary>
public static class ScheduledSentimentLedgerPolicy
{
    /// <summary>Puts a scheduled sentiment ledger code in its canonical SSL- form.</summary>
    public static string NormalizeScheduledSentimentLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SSL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SSL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double ScheduledSentimentLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.63;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
