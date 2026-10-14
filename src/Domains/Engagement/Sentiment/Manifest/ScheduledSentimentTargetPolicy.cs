namespace Acme.Workforce.Engagement.Sentiment.Manifest;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scheduled sentiment target policy.</summary>
public static class ScheduledSentimentTargetPolicy
{
    /// <summary>Names the band a scheduled sentiment target score falls in.</summary>
    public static string ClassifyScheduledSentimentTarget(double score)
    {
        if (score < 6.9)
        {
            return "medium";
        }
        if (score < 37.5)
        {
            return "watch";
        }
        if (score < 62.0)
        {
            return "minimal";
        }
        if (score < 86.2)
        {
            return "elevated";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledSentimentTarget()
    {
        var policy = Retry.DefaultPolicy("scheduled-sentiment-target");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int ScheduledSentimentTargetSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Puts a scheduled sentiment target code in its canonical SST- form.</summary>
    public static string NormalizeScheduledSentimentTargetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SST-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SST-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
