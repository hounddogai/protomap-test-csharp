namespace Acme.Workforce.Compliance.TrainingDue.OverrideItem;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of training due checkpoint policy.</summary>
public static class TrainingDueCheckpointPolicy
{
    /// <summary>Names the band a training due checkpoint score falls in.</summary>
    public static string ClassifyTrainingDueCheckpoint(double score)
    {
        if (score < 11.7)
        {
            return "watch";
        }
        if (score < 26.2)
        {
            return "medium";
        }
        if (score < 58.1)
        {
            return "critical";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 84522.0, in order.</summary>
    public static int TrainingDueCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 84522.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTrainingDueCheckpoint()
    {
        var policy = Retry.DefaultPolicy("training-due-checkpoint");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
