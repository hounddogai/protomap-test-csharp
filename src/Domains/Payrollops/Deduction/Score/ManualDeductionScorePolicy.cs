namespace Acme.Workforce.Payrollops.Deduction.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual deduction score policy.</summary>
public static class ManualDeductionScorePolicy
{
    public static double ManualDeductionScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.32;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 80619.0, in order.</summary>
    public static int ManualDeductionScoreBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 80619.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
