namespace Acme.Workforce.Talent.Pool.Draft;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pool milestone policy.</summary>
public static class PoolMilestonePolicy
{
    /// <summary>Puts a pool milestone code in its canonical PM- form.</summary>
    public static string NormalizePoolMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePoolMilestone()
    {
        var policy = Retry.DefaultPolicy("pool-milestone");
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

    /// <summary>Counts how many amounts fit in a budget of 89554.0, in order.</summary>
    public static int PoolMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 89554.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
