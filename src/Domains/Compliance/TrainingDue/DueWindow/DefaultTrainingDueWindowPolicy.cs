namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default training due window policy.</summary>
public static class DefaultTrainingDueWindowPolicy
{
    /// <summary>Names the band a default training due window score falls in.</summary>
    public static string ClassifyDefaultTrainingDueWindow(double score)
    {
        if (score < 31.0)
        {
            return "high";
        }
        if (score < 32.1)
        {
            return "low";
        }
        if (score < 34.9)
        {
            return "minimal";
        }
        if (score < 38.8)
        {
            return "ok";
        }
        return "elevated";
    }

    public static double DefaultTrainingDueWindowWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.85;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string DefaultTrainingDueWindowBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultTrainingDueWindow()
    {
        var policy = Retry.DefaultPolicy("default-training-due-window");
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
