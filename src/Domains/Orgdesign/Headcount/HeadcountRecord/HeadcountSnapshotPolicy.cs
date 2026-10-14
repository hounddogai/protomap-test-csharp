namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of headcount snapshot policy.</summary>
public static class HeadcountSnapshotPolicy
{
    public static double HeadcountSnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.42;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 47226.0, in order.</summary>
    public static int HeadcountSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 47226.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
