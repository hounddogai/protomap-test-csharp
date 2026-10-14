namespace Acme.Workforce.Absence.Balance.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of balance snapshot policy.</summary>
public static class BalanceSnapshotPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 22976.0, in order.</summary>
    public static int BalanceSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 22976.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string BalanceSnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
