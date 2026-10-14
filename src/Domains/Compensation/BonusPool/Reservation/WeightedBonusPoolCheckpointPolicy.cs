namespace Acme.Workforce.Compensation.BonusPool.Reservation;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted bonus pool checkpoint policy.</summary>
public static class WeightedBonusPoolCheckpointPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 71082.0, in order.</summary>
    public static int WeightedBonusPoolCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 71082.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string WeightedBonusPoolCheckpointBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedBonusPoolCheckpoint()
    {
        var policy = Retry.DefaultPolicy("weighted-bonus-pool-checkpoint");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
