namespace Acme.Workforce.Compliance.TrainingDue.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback training due rollup policy.</summary>
public static class FallbackTrainingDueRollupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 87292.0, in order.</summary>
    public static int FallbackTrainingDueRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 87292.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string FallbackTrainingDueRollupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
