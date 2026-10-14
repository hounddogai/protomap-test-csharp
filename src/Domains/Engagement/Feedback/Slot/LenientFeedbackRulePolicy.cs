namespace Acme.Workforce.Engagement.Feedback.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient feedback rule policy.</summary>
public static class LenientFeedbackRulePolicy
{
    public static double LenientFeedbackRuleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.49;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a lenient feedback rule code in its canonical LFR- form.</summary>
    public static string NormalizeLenientFeedbackRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LFR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LFR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
