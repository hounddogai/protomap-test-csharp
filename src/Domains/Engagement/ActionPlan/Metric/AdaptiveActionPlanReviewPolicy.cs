namespace Acme.Workforce.Engagement.ActionPlan.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive action plan review policy.</summary>
public static class AdaptiveActionPlanReviewPolicy
{
    /// <summary>Names the band a adaptive action plan review score falls in.</summary>
    public static string ClassifyAdaptiveActionPlanReview(double score)
    {
        if (score < 13.9)
        {
            return "medium";
        }
        if (score < 21.8)
        {
            return "low";
        }
        if (score < 54.2)
        {
            return "critical";
        }
        return "minimal";
    }

    public static string AdaptiveActionPlanReviewBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
