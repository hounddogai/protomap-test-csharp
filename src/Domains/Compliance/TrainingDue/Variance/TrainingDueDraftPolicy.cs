namespace Acme.Workforce.Compliance.TrainingDue.Variance;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of training due draft policy.</summary>
public static class TrainingDueDraftPolicy
{
    public static int TrainingDueDraftSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTrainingDueDraft()
    {
        var policy = Retry.DefaultPolicy("training-due-draft");
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
