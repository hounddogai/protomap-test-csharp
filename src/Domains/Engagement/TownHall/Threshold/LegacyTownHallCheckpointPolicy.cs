namespace Acme.Workforce.Engagement.TownHall.Threshold;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy town hall checkpoint policy.</summary>
public static class LegacyTownHallCheckpointPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 95163.0, in order.</summary>
    public static int LegacyTownHallCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 95163.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyTownHallCheckpoint()
    {
        var policy = Retry.DefaultPolicy("legacy-town-hall-checkpoint");
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
