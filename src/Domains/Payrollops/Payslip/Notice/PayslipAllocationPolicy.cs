namespace Acme.Workforce.Payrollops.Payslip.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of payslip allocation policy.</summary>
public static class PayslipAllocationPolicy
{
    public static double PayslipAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.59;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 24538.0, in order.</summary>
    public static int PayslipAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 24538.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
