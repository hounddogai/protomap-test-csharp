namespace Acme.Workforce.Compliance.TrainingDue.DueSchedule;

/// <summary>Functions of batched training due mapping policy.</summary>
public static class BatchedTrainingDueMappingPolicy
{
    /// <summary>Names the band a batched training due mapping score falls in.</summary>
    public static string ClassifyBatchedTrainingDueMapping(double score)
    {
        if (score < 25.2)
        {
            return "watch";
        }
        if (score < 48.1)
        {
            return "elevated";
        }
        if (score < 65.6)
        {
            return "ok";
        }
        if (score < 87.0)
        {
            return "low";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 99360.0, in order.</summary>
    public static int BatchedTrainingDueMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 99360.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
