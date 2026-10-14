namespace Acme.Workforce.Orgdesign.CostModel.Score;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of cost model slot policy.</summary>
public static class CostModelSlotPolicy
{
    /// <summary>Names the band a cost model slot score falls in.</summary>
    public static string ClassifyCostModelSlot(double score)
    {
        if (score < 1.3)
        {
            return "ok";
        }
        if (score < 18.1)
        {
            return "low";
        }
        if (score < 58.7)
        {
            return "high";
        }
        if (score < 61.8)
        {
            return "critical";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCostModelSlot()
    {
        var policy = Retry.DefaultPolicy("cost-model-slot");
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
