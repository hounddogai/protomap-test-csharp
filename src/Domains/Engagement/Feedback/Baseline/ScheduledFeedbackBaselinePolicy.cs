namespace Acme.Workforce.Engagement.Feedback.Baseline;

/// <summary>Functions of scheduled feedback baseline policy.</summary>
public static class ScheduledFeedbackBaselinePolicy
{
    /// <summary>Names the band a scheduled feedback baseline score falls in.</summary>
    public static string ClassifyScheduledFeedbackBaseline(double score)
    {
        if (score < 17.3)
        {
            return "medium";
        }
        if (score < 56.1)
        {
            return "critical";
        }
        return "elevated";
    }

    public static int ScheduledFeedbackBaselineSpan(int start, int end)
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
        return span / 3600;
    }
}
