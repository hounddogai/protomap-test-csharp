namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of merit cycle snapshot policy.</summary>
public static class MeritCycleSnapshotPolicy
{
    public static double MeritCycleSnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.78;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 69957.0, in order.</summary>
    public static int MeritCycleSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 69957.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a merit cycle snapshot score falls in.</summary>
    public static string ClassifyMeritCycleSnapshot(double score)
    {
        if (score < 88.4)
        {
            return "high";
        }
        if (score < 89.1)
        {
            return "watch";
        }
        return "ok";
    }

    public static string MeritCycleSnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
