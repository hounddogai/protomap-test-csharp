namespace Acme.Workforce.Payrollops.Payslip.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default payslip threshold policy.</summary>
public static class DefaultPayslipThresholdPolicy
{
    /// <summary>Puts a default payslip threshold code in its canonical DPT- form.</summary>
    public static string NormalizeDefaultPayslipThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DPT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DPT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string DefaultPayslipThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
