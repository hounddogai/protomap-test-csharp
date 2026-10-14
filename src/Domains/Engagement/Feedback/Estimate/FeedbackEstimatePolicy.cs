namespace Acme.Workforce.Engagement.Feedback.Estimate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of feedback estimate policy.</summary>
public static class FeedbackEstimatePolicy
{
    /// <summary>Names the band a feedback estimate score falls in.</summary>
    public static string ClassifyFeedbackEstimate(double score)
    {
        if (score < 10.5)
        {
            return "low";
        }
        if (score < 84.9)
        {
            return "critical";
        }
        if (score < 93.8)
        {
            return "ok";
        }
        return "high";
    }

    public static List<string> MergeFeedbackEstimateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a feedback estimate code in its canonical FE- form.</summary>
    public static string NormalizeFeedbackEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
