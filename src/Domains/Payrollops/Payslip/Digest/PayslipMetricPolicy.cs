namespace Acme.Workforce.Payrollops.Payslip.Digest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of payslip metric policy.</summary>
public static class PayslipMetricPolicy
{
    public static List<string> MergePayslipMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a payslip metric code in its canonical PM- form.</summary>
    public static string NormalizePayslipMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
