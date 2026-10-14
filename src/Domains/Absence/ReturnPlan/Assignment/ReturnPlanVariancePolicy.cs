namespace Acme.Workforce.Absence.ReturnPlan.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of return plan variance policy.</summary>
public static class ReturnPlanVariancePolicy
{
    /// <summary>Names the band a return plan variance score falls in.</summary>
    public static string ClassifyReturnPlanVariance(double score)
    {
        if (score < 22.7)
        {
            return "medium";
        }
        if (score < 47.6)
        {
            return "critical";
        }
        if (score < 93.4)
        {
            return "low";
        }
        return "high";
    }

    public static string ReturnPlanVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
