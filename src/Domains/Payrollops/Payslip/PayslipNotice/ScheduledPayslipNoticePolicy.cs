namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scheduled payslip notice policy.</summary>
public static class ScheduledPayslipNoticePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledPayslipNotice()
    {
        var policy = Retry.DefaultPolicy("scheduled-payslip-notice");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a scheduled payslip notice code in its canonical SPN- form.</summary>
    public static string NormalizeScheduledPayslipNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
