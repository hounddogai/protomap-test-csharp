namespace Acme.Workforce.Talent.Pool.Budget;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pool budget policy.</summary>
public static class PoolBudgetPolicy
{
    /// <summary>Names the band a pool budget score falls in.</summary>
    public static string ClassifyPoolBudget(double score)
    {
        if (score < 40.4)
        {
            return "low";
        }
        if (score < 47.2)
        {
            return "medium";
        }
        if (score < 74.8)
        {
            return "minimal";
        }
        return "watch";
    }

    public static double PoolBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.48;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePoolBudget()
    {
        var policy = Retry.DefaultPolicy("pool-budget");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
