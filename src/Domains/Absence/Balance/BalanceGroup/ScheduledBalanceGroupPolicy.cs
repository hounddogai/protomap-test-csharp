namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scheduled balance group policy.</summary>
public static class ScheduledBalanceGroupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 10691.0, in order.</summary>
    public static int ScheduledBalanceGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 10691.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a scheduled balance group code in its canonical SBG- form.</summary>
    public static string NormalizeScheduledBalanceGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SBG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SBG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int ScheduledBalanceGroupSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledBalanceGroup()
    {
        var policy = Retry.DefaultPolicy("scheduled-balance-group");
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
