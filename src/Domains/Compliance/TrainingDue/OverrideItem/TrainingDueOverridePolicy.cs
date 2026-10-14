namespace Acme.Workforce.Compliance.TrainingDue.OverrideItem;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of training due override policy.</summary>
public static class TrainingDueOverridePolicy
{
    public static int TrainingDueOverrideSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTrainingDueOverride()
    {
        var policy = Retry.DefaultPolicy("training-due-override");
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

    /// <summary>Names the band a training due override score falls in.</summary>
    public static string ClassifyTrainingDueOverride(double score)
    {
        if (score < 20.9)
        {
            return "watch";
        }
        if (score < 37.2)
        {
            return "minimal";
        }
        if (score < 74.7)
        {
            return "low";
        }
        return "critical";
    }

    public static string TrainingDueOverrideBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
