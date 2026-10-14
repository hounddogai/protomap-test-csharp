namespace Acme.Workforce.Payrollops.Deduction.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient deduction agreement policy.</summary>
public static class LenientDeductionAgreementPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 74259.0, in order.</summary>
    public static int LenientDeductionAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 74259.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double LenientDeductionAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.63;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
