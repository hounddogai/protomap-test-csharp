namespace Acme.Workforce.Compliance.TrainingDue.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback training due baseline policy.</summary>
public static class FallbackTrainingDueBaselinePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 26836.0, in order.</summary>
    public static int FallbackTrainingDueBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 26836.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string FallbackTrainingDueBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
