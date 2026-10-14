namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of feedback reservation policy.</summary>
public static class FeedbackReservationPolicy
{
    /// <summary>Puts a feedback reservation code in its canonical FR- form.</summary>
    public static string NormalizeFeedbackReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string FeedbackReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
