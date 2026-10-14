namespace Acme.Workforce.Payrollops.Payslip.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of payslip adjustment policy.</summary>
public static class PayslipAdjustmentPolicy
{
    public static string PayslipAdjustmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static int PayslipAdjustmentSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 30830.0, in order.</summary>
    public static int PayslipAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 30830.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
