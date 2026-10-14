namespace Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of payslip batch policy.</summary>
public static class PayslipBatchPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayslipBatch()
    {
        var policy = Retry.DefaultPolicy("payslip-batch");
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

    /// <summary>Puts a payslip batch code in its canonical PB- form.</summary>
    public static string NormalizePayslipBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a payslip batch score falls in.</summary>
    public static string ClassifyPayslipBatch(double score)
    {
        if (score < 11.5)
        {
            return "medium";
        }
        if (score < 36.0)
        {
            return "high";
        }
        return "minimal";
    }
}
