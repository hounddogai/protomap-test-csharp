namespace Acme.Workforce.Payrollops.Payslip.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of payslip assignment policy.</summary>
public static class PayslipAssignmentPolicy
{
    /// <summary>Puts a payslip assignment code in its canonical PA- form.</summary>
    public static string NormalizePayslipAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PayslipAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.77;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
