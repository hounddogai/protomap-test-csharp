namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of batched payslip claim policy.</summary>
public static class BatchedPayslipClaimPolicy
{
    /// <summary>Puts a batched payslip claim code in its canonical BPC- form.</summary>
    public static string NormalizeBatchedPayslipClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BPC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BPC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string BatchedPayslipClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static List<string> MergeBatchedPayslipClaimLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
