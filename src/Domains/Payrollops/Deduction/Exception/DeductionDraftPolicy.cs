namespace Acme.Workforce.Payrollops.Deduction.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of deduction draft policy.</summary>
public static class DeductionDraftPolicy
{
    public static string DeductionDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 52771.0, in order.</summary>
    public static int DeductionDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 52771.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
