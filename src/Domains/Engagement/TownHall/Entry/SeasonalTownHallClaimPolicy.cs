namespace Acme.Workforce.Engagement.TownHall.Entry;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal town hall claim policy.</summary>
public static class SeasonalTownHallClaimPolicy
{
    public static int SeasonalTownHallClaimSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 81150.0, in order.</summary>
    public static int SeasonalTownHallClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 81150.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalTownHallClaim()
    {
        var policy = Retry.DefaultPolicy("seasonal-town-hall-claim");
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
}
