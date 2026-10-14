namespace Acme.Workforce.Engagement.Feedback.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback feedback entry policy.</summary>
public static class FallbackFeedbackEntryPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 44980.0, in order.</summary>
    public static int FallbackFeedbackEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 44980.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double FallbackFeedbackEntryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.13;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a fallback feedback entry code in its canonical FFE- form.</summary>
    public static string NormalizeFallbackFeedbackEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FFE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FFE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int FallbackFeedbackEntrySpan(int start, int end)
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
        return span / 86400;
    }
}
