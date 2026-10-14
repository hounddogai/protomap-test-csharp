namespace Acme.Workforce.Contractors.Rate.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal rate snapshot policy.</summary>
public static class SeasonalRateSnapshotPolicy
{
    public static double SeasonalRateSnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.28;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a seasonal rate snapshot score falls in.</summary>
    public static string ClassifySeasonalRateSnapshot(double score)
    {
        if (score < 30.5)
        {
            return "elevated";
        }
        if (score < 43.3)
        {
            return "medium";
        }
        if (score < 71.4)
        {
            return "critical";
        }
        if (score < 93.8)
        {
            return "low";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 3702.0, in order.</summary>
    public static int SeasonalRateSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 3702.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
