namespace Acme.Workforce.Compensation.MeritCycle.Batch;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending merit cycle batch policy.</summary>
public static class PendingMeritCycleBatchPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 30478.0, in order.</summary>
    public static int PendingMeritCycleBatchBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 30478.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a pending merit cycle batch score falls in.</summary>
    public static string ClassifyPendingMeritCycleBatch(double score)
    {
        if (score < 76.1)
        {
            return "critical";
        }
        if (score < 92.6)
        {
            return "ok";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingMeritCycleBatch()
    {
        var policy = Retry.DefaultPolicy("pending-merit-cycle-batch");
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

    public static int PendingMeritCycleBatchSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }
}
