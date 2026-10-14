namespace Acme.Workforce.Absence.ReturnPlan.Window;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered return plan item policy.</summary>
public static class TieredReturnPlanItemPolicy
{
    /// <summary>Names the band a tiered return plan item score falls in.</summary>
    public static string ClassifyTieredReturnPlanItem(double score)
    {
        if (score < 33.0)
        {
            return "low";
        }
        if (score < 54.0)
        {
            return "critical";
        }
        if (score < 63.4)
        {
            return "minimal";
        }
        if (score < 99.1)
        {
            return "medium";
        }
        return "watch";
    }

    public static string TieredReturnPlanItemBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredReturnPlanItem()
    {
        var policy = Retry.DefaultPolicy("tiered-return-plan-item");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
