namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive training due exception policy.</summary>
public static class AdaptiveTrainingDueExceptionPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveTrainingDueException()
    {
        var policy = Retry.DefaultPolicy("adaptive-training-due-exception");
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

    /// <summary>Names the band a adaptive training due exception score falls in.</summary>
    public static string ClassifyAdaptiveTrainingDueException(double score)
    {
        if (score < 17.1)
        {
            return "ok";
        }
        if (score < 81.8)
        {
            return "medium";
        }
        if (score < 96.1)
        {
            return "low";
        }
        return "minimal";
    }
}
