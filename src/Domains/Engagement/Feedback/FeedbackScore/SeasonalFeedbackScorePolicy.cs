namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal feedback score policy.</summary>
public static class SeasonalFeedbackScorePolicy
{
    public static int SeasonalFeedbackScoreSpan(int start, int end)
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

    /// <summary>Puts a seasonal feedback score code in its canonical SFS- form.</summary>
    public static string NormalizeSeasonalFeedbackScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SFS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SFS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double SeasonalFeedbackScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.52;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
