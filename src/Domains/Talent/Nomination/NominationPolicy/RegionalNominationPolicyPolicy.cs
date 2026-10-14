namespace Acme.Workforce.Talent.Nomination.NominationPolicy;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional nomination policy policy.</summary>
public static class RegionalNominationPolicyPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 86291.0, in order.</summary>
    public static int RegionalNominationPolicyBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 86291.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalNominationPolicy()
    {
        var policy = Retry.DefaultPolicy("regional-nomination-policy");
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
