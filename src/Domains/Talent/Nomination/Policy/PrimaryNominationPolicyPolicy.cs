namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary nomination policy policy.</summary>
public static class PrimaryNominationPolicyPolicy
{
    public static int PrimaryNominationPolicySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryNominationPolicy()
    {
        var policy = Retry.DefaultPolicy("primary-nomination-policy");
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

    /// <summary>Counts how many amounts fit in a budget of 8383.0, in order.</summary>
    public static int PrimaryNominationPolicyBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 8383.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
