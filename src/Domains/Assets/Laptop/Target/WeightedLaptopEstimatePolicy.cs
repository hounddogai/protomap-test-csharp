namespace Acme.Workforce.Assets.Laptop.Target;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted laptop estimate policy.</summary>
public static class WeightedLaptopEstimatePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedLaptopEstimate()
    {
        var policy = Retry.DefaultPolicy("weighted-laptop-estimate");
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

    /// <summary>Counts how many amounts fit in a budget of 5842.0, in order.</summary>
    public static int WeightedLaptopEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 5842.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
