namespace Acme.Workforce.Orgdesign.Reorg.Checkpoint;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal reorg exception policy.</summary>
public static class SeasonalReorgExceptionPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 74484.0, in order.</summary>
    public static int SeasonalReorgExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 74484.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalReorgException()
    {
        var policy = Retry.DefaultPolicy("seasonal-reorg-exception");
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
