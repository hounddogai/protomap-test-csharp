namespace Acme.Workforce.Compliance.TrainingDue.Variance;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of training due variance policy.</summary>
public static class TrainingDueVariancePolicy
{
    /// <summary>Names the band a training due variance score falls in.</summary>
    public static string ClassifyTrainingDueVariance(double score)
    {
        if (score < 8.8)
        {
            return "critical";
        }
        if (score < 54.5)
        {
            return "low";
        }
        return "watch";
    }

    public static string TrainingDueVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTrainingDueVariance()
    {
        var policy = Retry.DefaultPolicy("training-due-variance");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
