namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of feedback threshold policy.</summary>
public static class FeedbackThresholdPolicy
{
    public static double FeedbackThresholdWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.21;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a feedback threshold code in its canonical FT- form.</summary>
    public static string NormalizeFeedbackThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string FeedbackThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
