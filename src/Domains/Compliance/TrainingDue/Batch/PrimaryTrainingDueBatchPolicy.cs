namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary training due batch policy.</summary>
public static class PrimaryTrainingDueBatchPolicy
{
    /// <summary>Puts a primary training due batch code in its canonical PTDB- form.</summary>
    public static string NormalizePrimaryTrainingDueBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PTDB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PTDB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryTrainingDueBatch()
    {
        var policy = Retry.DefaultPolicy("primary-training-due-batch");
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

    /// <summary>Names the band a primary training due batch score falls in.</summary>
    public static string ClassifyPrimaryTrainingDueBatch(double score)
    {
        if (score < 38.4)
        {
            return "ok";
        }
        if (score < 60.6)
        {
            return "watch";
        }
        if (score < 88.0)
        {
            return "minimal";
        }
        if (score < 99.0)
        {
            return "low";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 17436.0, in order.</summary>
    public static int PrimaryTrainingDueBatchBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 17436.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
