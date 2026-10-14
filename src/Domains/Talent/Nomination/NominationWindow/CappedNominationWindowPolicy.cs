namespace Acme.Workforce.Talent.Nomination.NominationWindow;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped nomination window policy.</summary>
public static class CappedNominationWindowPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 15100.0, in order.</summary>
    public static int CappedNominationWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 15100.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string CappedNominationWindowBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
