namespace Acme.Workforce.Workplace.Desk.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of desk snapshot policy.</summary>
public static class DeskSnapshotPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 76339.0, in order.</summary>
    public static int DeskSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 76339.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string DeskSnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
