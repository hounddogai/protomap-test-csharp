namespace Acme.Workforce.Compliance.TrainingDue.Notice;

/// <summary>Functions of capped training due target policy.</summary>
public static class CappedTrainingDueTargetPolicy
{
    public static int CappedTrainingDueTargetSpan(int start, int end)
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

    /// <summary>Names the band a capped training due target score falls in.</summary>
    public static string ClassifyCappedTrainingDueTarget(double score)
    {
        if (score < 33.7)
        {
            return "ok";
        }
        if (score < 42.9)
        {
            return "elevated";
        }
        if (score < 85.4)
        {
            return "medium";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 28622.0, in order.</summary>
    public static int CappedTrainingDueTargetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 28622.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
