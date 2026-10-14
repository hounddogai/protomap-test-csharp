namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of training due baseline policy.</summary>
public static class TrainingDueBaselinePolicy
{
    /// <summary>Names the band a training due baseline score falls in.</summary>
    public static string ClassifyTrainingDueBaseline(double score)
    {
        if (score < 36.5)
        {
            return "ok";
        }
        if (score < 78.7)
        {
            return "high";
        }
        if (score < 82.8)
        {
            return "elevated";
        }
        return "critical";
    }

    public static string TrainingDueBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static int TrainingDueBaselineSpan(int start, int end)
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
    public static int ProbeTrainingDueBaseline()
    {
        var policy = Retry.DefaultPolicy("training-due-baseline");
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
