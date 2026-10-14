namespace Acme.Workforce.Engagement.ActionPlan.Claim;

/// <summary>Functions of lenient action plan exception policy.</summary>
public static class LenientActionPlanExceptionPolicy
{
    public static int LenientActionPlanExceptionSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Names the band a lenient action plan exception score falls in.</summary>
    public static string ClassifyLenientActionPlanException(double score)
    {
        if (score < 12.1)
        {
            return "medium";
        }
        if (score < 83.3)
        {
            return "ok";
        }
        if (score < 84.3)
        {
            return "critical";
        }
        return "high";
    }
}
