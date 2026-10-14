namespace Acme.Workforce.Engagement.Feedback.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal feedback milestone policy.</summary>
public static class SeasonalFeedbackMilestonePolicy
{
    public static string SeasonalFeedbackMilestoneBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a seasonal feedback milestone code in its canonical SFM- form.</summary>
    public static string NormalizeSeasonalFeedbackMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SFM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SFM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
