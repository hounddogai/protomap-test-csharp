namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of succession plan audit policy.</summary>
public static class SuccessionPlanAuditPolicy
{
    /// <summary>Names the band a succession plan audit score falls in.</summary>
    public static string ClassifySuccessionPlanAudit(double score)
    {
        if (score < 10.2)
        {
            return "medium";
        }
        if (score < 82.8)
        {
            return "low";
        }
        if (score < 83.3)
        {
            return "ok";
        }
        if (score < 90.6)
        {
            return "watch";
        }
        return "critical";
    }

    public static string SuccessionPlanAuditBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double SuccessionPlanAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.12;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
