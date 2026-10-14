namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of merit cycle checkpoint policy.</summary>
public static class MeritCycleCheckpointPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 75124.0, in order.</summary>
    public static int MeritCycleCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 75124.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string MeritCycleCheckpointBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a merit cycle checkpoint score falls in.</summary>
    public static string ClassifyMeritCycleCheckpoint(double score)
    {
        if (score < 49.3)
        {
            return "medium";
        }
        if (score < 52.3)
        {
            return "elevated";
        }
        if (score < 68.8)
        {
            return "minimal";
        }
        if (score < 89.4)
        {
            return "watch";
        }
        return "ok";
    }
}
