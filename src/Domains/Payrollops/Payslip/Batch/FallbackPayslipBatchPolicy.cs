namespace Acme.Workforce.Payrollops.Payslip.Batch;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback payslip batch policy.</summary>
public static class FallbackPayslipBatchPolicy
{
    public static double FallbackPayslipBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.78;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackPayslipBatch()
    {
        var policy = Retry.DefaultPolicy("fallback-payslip-batch");
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

    public static string FallbackPayslipBatchBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a fallback payslip batch code in its canonical FPB- form.</summary>
    public static string NormalizeFallbackPayslipBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FPB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FPB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
