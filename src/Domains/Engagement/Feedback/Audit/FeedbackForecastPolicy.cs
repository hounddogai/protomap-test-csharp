namespace Acme.Workforce.Engagement.Feedback.Audit;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of feedback forecast policy.</summary>
public static class FeedbackForecastPolicy
{
    /// <summary>Puts a feedback forecast code in its canonical FF- form.</summary>
    public static string NormalizeFeedbackForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int FeedbackForecastSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    public static double FeedbackForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.41;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeFeedbackForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
