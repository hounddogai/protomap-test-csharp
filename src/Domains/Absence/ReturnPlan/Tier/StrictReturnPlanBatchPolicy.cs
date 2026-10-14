namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict return plan batch policy.</summary>
public static class StrictReturnPlanBatchPolicy
{
    public static string StrictReturnPlanBatchBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictReturnPlanBatch()
    {
        var policy = Retry.DefaultPolicy("strict-return-plan-batch");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a strict return plan batch score falls in.</summary>
    public static string ClassifyStrictReturnPlanBatch(double score)
    {
        if (score < 41.1)
        {
            return "ok";
        }
        if (score < 58.9)
        {
            return "elevated";
        }
        return "watch";
    }
}
