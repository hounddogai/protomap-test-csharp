namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback sentiment preference policy.</summary>
public static class FallbackSentimentPreferencePolicy
{
    /// <summary>Puts a fallback sentiment preference code in its canonical FSP- form.</summary>
    public static string NormalizeFallbackSentimentPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FSP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FSP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackSentimentPreference()
    {
        var policy = Retry.DefaultPolicy("fallback-sentiment-preference");
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

    public static int FallbackSentimentPreferenceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }
}
