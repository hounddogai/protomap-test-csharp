namespace Acme.Workforce.Payrollops.Payslip.Digest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of payslip digest policy.</summary>
public static class PayslipDigestPolicy
{
    /// <summary>Puts a payslip digest code in its canonical PD- form.</summary>
    public static string NormalizePayslipDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PayslipDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.4;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergePayslipDigestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
