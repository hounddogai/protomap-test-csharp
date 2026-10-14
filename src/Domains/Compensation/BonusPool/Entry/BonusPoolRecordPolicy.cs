namespace Acme.Workforce.Compensation.BonusPool.Entry;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of bonus pool record policy.</summary>
public static class BonusPoolRecordPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 67117.0, in order.</summary>
    public static int BonusPoolRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 67117.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBonusPoolRecord()
    {
        var policy = Retry.DefaultPolicy("bonus-pool-record");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
