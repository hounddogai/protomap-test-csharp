namespace Acme.Workforce.Assets.Laptop.Bundle;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of laptop rollup policy.</summary>
public static class LaptopRollupPolicy
{
    /// <summary>Names the band a laptop rollup score falls in.</summary>
    public static string ClassifyLaptopRollup(double score)
    {
        if (score < 41.1)
        {
            return "high";
        }
        if (score < 68.2)
        {
            return "watch";
        }
        if (score < 78.9)
        {
            return "critical";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLaptopRollup()
    {
        var policy = Retry.DefaultPolicy("laptop-rollup");
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
}
