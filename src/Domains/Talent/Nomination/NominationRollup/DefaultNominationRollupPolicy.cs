namespace Acme.Workforce.Talent.Nomination.NominationRollup;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default nomination rollup policy.</summary>
public static class DefaultNominationRollupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 74609.0, in order.</summary>
    public static int DefaultNominationRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 74609.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultNominationRollup()
    {
        var policy = Retry.DefaultPolicy("default-nomination-rollup");
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
