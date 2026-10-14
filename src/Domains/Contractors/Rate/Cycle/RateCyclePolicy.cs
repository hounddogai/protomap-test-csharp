namespace Acme.Workforce.Contractors.Rate.Cycle;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rate cycle policy.</summary>
public static class RateCyclePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 35015.0, in order.</summary>
    public static int RateCycleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 35015.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRateCycle()
    {
        var policy = Retry.DefaultPolicy("rate-cycle");
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
