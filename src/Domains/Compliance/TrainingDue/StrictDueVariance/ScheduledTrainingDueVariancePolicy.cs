namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scheduled training due variance policy.</summary>
public static class ScheduledTrainingDueVariancePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledTrainingDueVariance()
    {
        var policy = Retry.DefaultPolicy("scheduled-training-due-variance");
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

    public static double ScheduledTrainingDueVarianceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.02;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a scheduled training due variance score falls in.</summary>
    public static string ClassifyScheduledTrainingDueVariance(double score)
    {
        if (score < 14.0)
        {
            return "minimal";
        }
        if (score < 81.1)
        {
            return "elevated";
        }
        if (score < 89.4)
        {
            return "medium";
        }
        return "watch";
    }

    public static string ScheduledTrainingDueVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
