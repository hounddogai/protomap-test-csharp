namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of payslip budget policy.</summary>
public static class PayslipBudgetPolicy
{
    public static int PayslipBudgetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a payslip budget score falls in.</summary>
    public static string ClassifyPayslipBudget(double score)
    {
        if (score < 41.1)
        {
            return "minimal";
        }
        if (score < 48.8)
        {
            return "medium";
        }
        if (score < 66.2)
        {
            return "watch";
        }
        if (score < 70.7)
        {
            return "critical";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayslipBudget()
    {
        var policy = Retry.DefaultPolicy("payslip-budget");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 90890.0, in order.</summary>
    public static int PayslipBudgetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 90890.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
