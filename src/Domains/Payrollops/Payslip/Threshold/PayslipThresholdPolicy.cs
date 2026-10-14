namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of payslip threshold policy.</summary>
public static class PayslipThresholdPolicy
{
    public static string PayslipThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a payslip threshold code in its canonical PT- form.</summary>
    public static string NormalizePayslipThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergePayslipThresholdLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int PayslipThresholdSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }
}
