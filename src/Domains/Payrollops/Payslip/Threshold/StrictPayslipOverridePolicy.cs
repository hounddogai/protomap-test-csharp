namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict payslip override policy.</summary>
public static class StrictPayslipOverridePolicy
{
    public static string StrictPayslipOverrideBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static int StrictPayslipOverrideSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictPayslipOverride()
    {
        var policy = Retry.DefaultPolicy("strict-payslip-override");
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

    /// <summary>Puts a strict payslip override code in its canonical SPO- form.</summary>
    public static string NormalizeStrictPayslipOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
