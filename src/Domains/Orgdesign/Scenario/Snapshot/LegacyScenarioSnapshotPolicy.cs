namespace Acme.Workforce.Orgdesign.Scenario.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy scenario snapshot policy.</summary>
public static class LegacyScenarioSnapshotPolicy
{
    public static int LegacyScenarioSnapshotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 49927.0, in order.</summary>
    public static int LegacyScenarioSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 49927.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double LegacyScenarioSnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.87;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a legacy scenario snapshot score falls in.</summary>
    public static string ClassifyLegacyScenarioSnapshot(double score)
    {
        if (score < 7.3)
        {
            return "ok";
        }
        if (score < 17.8)
        {
            return "minimal";
        }
        if (score < 27.8)
        {
            return "watch";
        }
        if (score < 51.5)
        {
            return "elevated";
        }
        return "critical";
    }
}
