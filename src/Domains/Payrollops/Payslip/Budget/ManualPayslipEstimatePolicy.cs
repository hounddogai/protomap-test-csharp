namespace Acme.Workforce.Payrollops.Payslip.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual payslip estimate policy.</summary>
public static class ManualPayslipEstimatePolicy
{
    public static double ManualPayslipEstimateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.15;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 99533.0, in order.</summary>
    public static int ManualPayslipEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 99533.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
