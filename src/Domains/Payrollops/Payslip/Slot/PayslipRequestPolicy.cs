namespace Acme.Workforce.Payrollops.Payslip.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of payslip request policy.</summary>
public static class PayslipRequestPolicy
{
    public static double PayslipRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.67;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a payslip request score falls in.</summary>
    public static string ClassifyPayslipRequest(double score)
    {
        if (score < 25.8)
        {
            return "high";
        }
        if (score < 98.0)
        {
            return "watch";
        }
        return "ok";
    }

    /// <summary>Puts a payslip request code in its canonical PR- form.</summary>
    public static string NormalizePayslipRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
