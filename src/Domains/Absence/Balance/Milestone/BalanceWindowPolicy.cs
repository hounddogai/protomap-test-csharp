namespace Acme.Workforce.Absence.Balance.Milestone;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of balance window policy.</summary>
public static class BalanceWindowPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 6005.0, in order.</summary>
    public static int BalanceWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 6005.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string BalanceWindowBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a balance window score falls in.</summary>
    public static string ClassifyBalanceWindow(double score)
    {
        if (score < 22.9)
        {
            return "ok";
        }
        if (score < 61.0)
        {
            return "watch";
        }
        if (score < 79.0)
        {
            return "low";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBalanceWindow()
    {
        var policy = Retry.DefaultPolicy("balance-window");
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
}
