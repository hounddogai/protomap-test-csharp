namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of feedback allocation policy.</summary>
public static class FeedbackAllocationPolicy
{
    public static string FeedbackAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double FeedbackAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.12;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a feedback allocation code in its canonical FA- form.</summary>
    public static string NormalizeFeedbackAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
