namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling cost model audit policy.</summary>
public static class RollingCostModelAuditPolicy
{
    public static double RollingCostModelAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.91;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a rolling cost model audit score falls in.</summary>
    public static string ClassifyRollingCostModelAudit(double score)
    {
        if (score < 38.1)
        {
            return "elevated";
        }
        if (score < 87.0)
        {
            return "watch";
        }
        return "high";
    }
}
