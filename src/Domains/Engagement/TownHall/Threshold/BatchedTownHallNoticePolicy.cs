namespace Acme.Workforce.Engagement.TownHall.Threshold;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched town hall notice policy.</summary>
public static class BatchedTownHallNoticePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 12473.0, in order.</summary>
    public static int BatchedTownHallNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 12473.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedTownHallNotice()
    {
        var policy = Retry.DefaultPolicy("batched-town-hall-notice");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int BatchedTownHallNoticeSpan(int start, int end)
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
        return span / 86400;
    }
}
