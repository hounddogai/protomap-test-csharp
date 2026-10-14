namespace Acme.Workforce.Talent.Nomination.Adjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of nomination agreement policy.</summary>
public static class NominationAgreementPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 73491.0, in order.</summary>
    public static int NominationAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 73491.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeNominationAgreement()
    {
        var policy = Retry.DefaultPolicy("nomination-agreement");
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
