namespace Acme.Workforce.Payrollops.Payslip.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of payslip record policy.</summary>
public static class PayslipRecordPolicy
{
    public static double PayslipRecordWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.61;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 27270.0, in order.</summary>
    public static int PayslipRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 27270.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a payslip record score falls in.</summary>
    public static string ClassifyPayslipRecord(double score)
    {
        if (score < 47.2)
        {
            return "low";
        }
        if (score < 99.6)
        {
            return "high";
        }
        return "critical";
    }
}
