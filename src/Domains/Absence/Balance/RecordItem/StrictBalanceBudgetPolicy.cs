namespace Acme.Workforce.Absence.Balance.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict balance budget policy.</summary>
public static class StrictBalanceBudgetPolicy
{
    public static string StrictBalanceBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static int StrictBalanceBudgetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 84974.0, in order.</summary>
    public static int StrictBalanceBudgetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 84974.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
