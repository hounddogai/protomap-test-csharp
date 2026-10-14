namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy training due variance policy.</summary>
public static class LegacyTrainingDueVariancePolicy
{
    public static int LegacyTrainingDueVarianceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyTrainingDueVariance()
    {
        var policy = Retry.DefaultPolicy("legacy-training-due-variance");
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

    /// <summary>Counts how many amounts fit in a budget of 23297.0, in order.</summary>
    public static int LegacyTrainingDueVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 23297.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a legacy training due variance score falls in.</summary>
    public static string ClassifyLegacyTrainingDueVariance(double score)
    {
        if (score < 64.5)
        {
            return "watch";
        }
        if (score < 73.4)
        {
            return "critical";
        }
        if (score < 76.2)
        {
            return "ok";
        }
        return "medium";
    }
}
