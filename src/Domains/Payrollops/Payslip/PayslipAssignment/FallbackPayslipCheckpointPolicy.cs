namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback payslip checkpoint policy.</summary>
public static class FallbackPayslipCheckpointPolicy
{
    /// <summary>Puts a fallback payslip checkpoint code in its canonical FPC- form.</summary>
    public static string NormalizeFallbackPayslipCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FPC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FPC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeFallbackPayslipCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
