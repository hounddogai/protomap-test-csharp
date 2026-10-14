namespace Acme.Workforce.Engagement.Sentiment.Bundle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback sentiment bundle policy.</summary>
public static class FallbackSentimentBundlePolicy
{
    public static List<string> MergeFallbackSentimentBundleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int FallbackSentimentBundleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Puts a fallback sentiment bundle code in its canonical FSB- form.</summary>
    public static string NormalizeFallbackSentimentBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FSB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FSB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackSentimentBundle()
    {
        var policy = Retry.DefaultPolicy("fallback-sentiment-bundle");
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
}
