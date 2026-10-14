namespace Acme.Workforce.Absence.Balance.Group;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of balance rule policy.</summary>
public static class BalanceRulePolicy
{
    public static string BalanceRuleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 88116.0, in order.</summary>
    public static int BalanceRuleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 88116.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a balance rule score falls in.</summary>
    public static string ClassifyBalanceRule(double score)
    {
        if (score < 32.3)
        {
            return "critical";
        }
        if (score < 98.8)
        {
            return "ok";
        }
        return "watch";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBalanceRule()
    {
        var policy = Retry.DefaultPolicy("balance-rule");
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
