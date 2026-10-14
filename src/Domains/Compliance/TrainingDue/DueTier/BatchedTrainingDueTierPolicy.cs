namespace Acme.Workforce.Compliance.TrainingDue.DueTier;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched training due tier policy.</summary>
public static class BatchedTrainingDueTierPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 46314.0, in order.</summary>
    public static int BatchedTrainingDueTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 46314.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int BatchedTrainingDueTierSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedTrainingDueTier()
    {
        var policy = Retry.DefaultPolicy("batched-training-due-tier");
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
