namespace Acme.Workforce.Absence.Balance.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional balance digest policy.</summary>
public static class RegionalBalanceDigestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 15531.0, in order.</summary>
    public static int RegionalBalanceDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 15531.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string RegionalBalanceDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static int RegionalBalanceDigestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }
}
