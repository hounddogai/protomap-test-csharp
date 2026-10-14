namespace Acme.Workforce.Engagement.Feedback.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary feedback notice policy.</summary>
public static class PrimaryFeedbackNoticePolicy
{
    public static double PrimaryFeedbackNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.34;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a primary feedback notice code in its canonical PFN- form.</summary>
    public static string NormalizePrimaryFeedbackNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PFN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PFN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a primary feedback notice score falls in.</summary>
    public static string ClassifyPrimaryFeedbackNotice(double score)
    {
        if (score < 38.4)
        {
            return "minimal";
        }
        if (score < 39.5)
        {
            return "high";
        }
        if (score < 43.4)
        {
            return "low";
        }
        if (score < 71.6)
        {
            return "medium";
        }
        return "critical";
    }

    public static int PrimaryFeedbackNoticeSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }
}
