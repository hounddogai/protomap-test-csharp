namespace Acme.Workforce.Engagement.Feedback.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of feedback summary policy.</summary>
public static class FeedbackSummaryPolicy
{
    public static double FeedbackSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.96;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a feedback summary code in its canonical FS- form.</summary>
    public static string NormalizeFeedbackSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
