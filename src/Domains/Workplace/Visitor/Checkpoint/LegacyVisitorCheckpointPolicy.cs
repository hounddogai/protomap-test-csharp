namespace Acme.Workforce.Workplace.Visitor.Checkpoint;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy visitor checkpoint policy.</summary>
public static class LegacyVisitorCheckpointPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyVisitorCheckpoint()
    {
        var policy = Retry.DefaultPolicy("legacy-visitor-checkpoint");
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

    /// <summary>Names the band a legacy visitor checkpoint score falls in.</summary>
    public static string ClassifyLegacyVisitorCheckpoint(double score)
    {
        if (score < 11.0)
        {
            return "critical";
        }
        if (score < 79.9)
        {
            return "elevated";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 1496.0, in order.</summary>
    public static int LegacyVisitorCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 1496.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
