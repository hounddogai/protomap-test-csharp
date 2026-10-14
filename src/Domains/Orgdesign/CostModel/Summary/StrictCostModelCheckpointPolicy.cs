namespace Acme.Workforce.Orgdesign.CostModel.Summary;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict cost model checkpoint policy.</summary>
public static class StrictCostModelCheckpointPolicy
{
    /// <summary>Names the band a strict cost model checkpoint score falls in.</summary>
    public static string ClassifyStrictCostModelCheckpoint(double score)
    {
        if (score < 25.1)
        {
            return "minimal";
        }
        if (score < 46.3)
        {
            return "high";
        }
        if (score < 63.3)
        {
            return "watch";
        }
        return "low";
    }

    public static double StrictCostModelCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.89;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictCostModelCheckpoint()
    {
        var policy = Retry.DefaultPolicy("strict-cost-model-checkpoint");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
