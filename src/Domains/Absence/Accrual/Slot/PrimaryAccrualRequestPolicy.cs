namespace Acme.Workforce.Absence.Accrual.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary accrual request policy.</summary>
public static class PrimaryAccrualRequestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 80294.0, in order.</summary>
    public static int PrimaryAccrualRequestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 80294.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double PrimaryAccrualRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.08;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string PrimaryAccrualRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
