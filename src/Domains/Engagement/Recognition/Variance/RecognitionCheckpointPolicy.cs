namespace Acme.Workforce.Engagement.Recognition.Variance;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of recognition checkpoint policy.</summary>
public static class RecognitionCheckpointPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRecognitionCheckpoint()
    {
        var policy = Retry.DefaultPolicy("recognition-checkpoint");
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

    /// <summary>Counts how many amounts fit in a budget of 60225.0, in order.</summary>
    public static int RecognitionCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 60225.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
