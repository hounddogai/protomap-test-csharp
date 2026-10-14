namespace Acme.Workforce.Absence.Balance.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched balance rule policy.</summary>
public static class BatchedBalanceRulePolicy
{
    public static string BatchedBalanceRuleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static int BatchedBalanceRuleSpan(int start, int end)
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

    /// <summary>Counts how many amounts fit in a budget of 67770.0, in order.</summary>
    public static int BatchedBalanceRuleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 67770.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
