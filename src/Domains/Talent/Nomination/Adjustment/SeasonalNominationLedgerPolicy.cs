namespace Acme.Workforce.Talent.Nomination.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal nomination ledger policy.</summary>
public static class SeasonalNominationLedgerPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 36328.0, in order.</summary>
    public static int SeasonalNominationLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 36328.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double SeasonalNominationLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.88;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string SeasonalNominationLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
