namespace Acme.Workforce.Assets.Refresh.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive refresh checkpoint policy.</summary>
public static class AdaptiveRefreshCheckpointPolicy
{
    /// <summary>Names the band a adaptive refresh checkpoint score falls in.</summary>
    public static string ClassifyAdaptiveRefreshCheckpoint(double score)
    {
        if (score < 45.5)
        {
            return "medium";
        }
        if (score < 61.7)
        {
            return "low";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 94056.0, in order.</summary>
    public static int AdaptiveRefreshCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 94056.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string AdaptiveRefreshCheckpointBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
