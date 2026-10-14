namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

/// <summary>Functions of global payslip notice policy.</summary>
public static class GlobalPayslipNoticePolicy
{
    /// <summary>Puts a global payslip notice code in its canonical GPN- form.</summary>
    public static string NormalizeGlobalPayslipNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GPN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GPN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 48759.0, in order.</summary>
    public static int GlobalPayslipNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 48759.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int GlobalPayslipNoticeSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
