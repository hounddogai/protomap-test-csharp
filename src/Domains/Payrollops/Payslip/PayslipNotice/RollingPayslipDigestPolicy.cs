namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling payslip digest policy.</summary>
public static class RollingPayslipDigestPolicy
{
    public static int RollingPayslipDigestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    public static double RollingPayslipDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.75;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 5506.0, in order.</summary>
    public static int RollingPayslipDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 5506.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
