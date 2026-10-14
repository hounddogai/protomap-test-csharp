namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending return plan rollup policy.</summary>
public static class PendingReturnPlanRollupPolicy
{
    public static string PendingReturnPlanRollupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double PendingReturnPlanRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.43;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
