namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of payslip exception policy.</summary>
public static class PayslipExceptionPolicy
{
    /// <summary>Names the band a payslip exception score falls in.</summary>
    public static string ClassifyPayslipException(double score)
    {
        if (score < 2.6)
        {
            return "low";
        }
        if (score < 88.1)
        {
            return "high";
        }
        return "critical";
    }

    public static int PayslipExceptionSpan(int start, int end)
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

    /// <summary>Counts how many amounts fit in a budget of 69716.0, in order.</summary>
    public static int PayslipExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 69716.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayslipException()
    {
        var policy = Retry.DefaultPolicy("payslip-exception");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
