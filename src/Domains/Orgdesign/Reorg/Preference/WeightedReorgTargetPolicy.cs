namespace Acme.Workforce.Orgdesign.Reorg.Preference;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted reorg target policy.</summary>
public static class WeightedReorgTargetPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 75271.0, in order.</summary>
    public static int WeightedReorgTargetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 75271.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a weighted reorg target score falls in.</summary>
    public static string ClassifyWeightedReorgTarget(double score)
    {
        if (score < 9.8)
        {
            return "critical";
        }
        if (score < 86.5)
        {
            return "low";
        }
        return "elevated";
    }

    public static string WeightedReorgTargetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedReorgTarget()
    {
        var policy = Retry.DefaultPolicy("weighted-reorg-target");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
