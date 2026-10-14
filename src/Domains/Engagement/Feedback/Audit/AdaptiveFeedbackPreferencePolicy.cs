namespace Acme.Workforce.Engagement.Feedback.Audit;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive feedback preference policy.</summary>
public static class AdaptiveFeedbackPreferencePolicy
{
    public static List<string> MergeAdaptiveFeedbackPreferenceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double AdaptiveFeedbackPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.13;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a adaptive feedback preference score falls in.</summary>
    public static string ClassifyAdaptiveFeedbackPreference(double score)
    {
        if (score < 14.3)
        {
            return "elevated";
        }
        if (score < 41.3)
        {
            return "high";
        }
        if (score < 45.1)
        {
            return "ok";
        }
        if (score < 64.7)
        {
            return "low";
        }
        return "minimal";
    }
}
