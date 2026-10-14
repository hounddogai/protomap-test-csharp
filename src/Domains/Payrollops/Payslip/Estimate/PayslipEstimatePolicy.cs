namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of payslip estimate policy.</summary>
public static class PayslipEstimatePolicy
{
    /// <summary>Puts a payslip estimate code in its canonical PE- form.</summary>
    public static string NormalizePayslipEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string PayslipEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayslipEstimate()
    {
        var policy = Retry.DefaultPolicy("payslip-estimate");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
