namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback training due batch policy.</summary>
public static class FallbackTrainingDueBatchPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 48491.0, in order.</summary>
    public static int FallbackTrainingDueBatchBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 48491.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a fallback training due batch score falls in.</summary>
    public static string ClassifyFallbackTrainingDueBatch(double score)
    {
        if (score < 23.6)
        {
            return "medium";
        }
        if (score < 23.9)
        {
            return "minimal";
        }
        if (score < 81.7)
        {
            return "ok";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackTrainingDueBatch()
    {
        var policy = Retry.DefaultPolicy("fallback-training-due-batch");
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

    public static double FallbackTrainingDueBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.76;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
