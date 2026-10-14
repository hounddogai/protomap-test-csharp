namespace Acme.Workforce.Absence.ReturnPlan.Template;

/// <summary>Functions of return plan template policy.</summary>
public static class ReturnPlanTemplatePolicy
{
    /// <summary>Names the band a return plan template score falls in.</summary>
    public static string ClassifyReturnPlanTemplate(double score)
    {
        if (score < 29.3)
        {
            return "minimal";
        }
        if (score < 55.2)
        {
            return "medium";
        }
        if (score < 58.3)
        {
            return "ok";
        }
        return "low";
    }

    public static int ReturnPlanTemplateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }
}
