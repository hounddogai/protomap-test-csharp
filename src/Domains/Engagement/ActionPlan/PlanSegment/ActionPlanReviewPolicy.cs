namespace Acme.Workforce.Engagement.ActionPlan.PlanSegment;

/// <summary>Functions of action plan review policy.</summary>
public static class ActionPlanReviewPolicy
{
    /// <summary>Names the band a action plan review score falls in.</summary>
    public static string ClassifyActionPlanReview(double score)
    {
        if (score < 36.6)
        {
            return "medium";
        }
        if (score < 41.8)
        {
            return "high";
        }
        if (score < 96.4)
        {
            return "elevated";
        }
        return "ok";
    }

    public static int ActionPlanReviewSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }
}
