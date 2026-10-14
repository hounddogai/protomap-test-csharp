namespace Acme.Workforce.Payrollops.Payslip.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted payslip budget policy.</summary>
public static class WeightedPayslipBudgetPolicy
{
    public static int WeightedPayslipBudgetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    public static double WeightedPayslipBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.82;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 86224.0, in order.</summary>
    public static int WeightedPayslipBudgetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 86224.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
