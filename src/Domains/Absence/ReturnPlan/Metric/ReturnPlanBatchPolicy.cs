namespace Acme.Workforce.Absence.ReturnPlan.Metric;

/// <summary>Functions of return plan batch policy.</summary>
public static class ReturnPlanBatchPolicy
{
    public static int ReturnPlanBatchSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Names the band a return plan batch score falls in.</summary>
    public static string ClassifyReturnPlanBatch(double score)
    {
        if (score < 56.8)
        {
            return "ok";
        }
        if (score < 81.5)
        {
            return "elevated";
        }
        if (score < 84.4)
        {
            return "low";
        }
        if (score < 95.3)
        {
            return "high";
        }
        return "minimal";
    }
}
