namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal sentiment draft policy.</summary>
public static class SeasonalSentimentDraftPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalSentimentDraft()
    {
        var policy = Retry.DefaultPolicy("seasonal-sentiment-draft");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a seasonal sentiment draft code in its canonical SSD- form.</summary>
    public static string NormalizeSeasonalSentimentDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SSD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SSD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
