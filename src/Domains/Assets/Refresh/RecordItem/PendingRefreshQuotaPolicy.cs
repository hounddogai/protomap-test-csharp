namespace Acme.Workforce.Assets.Refresh.RecordItem;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending refresh quota policy.</summary>
public static class PendingRefreshQuotaPolicy
{
    public static int PendingRefreshQuotaSpan(int start, int end)
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
    public static int ProbePendingRefreshQuota()
    {
        var policy = Retry.DefaultPolicy("pending-refresh-quota");
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

    /// <summary>Counts how many amounts fit in a budget of 32475.0, in order.</summary>
    public static int PendingRefreshQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 32475.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
