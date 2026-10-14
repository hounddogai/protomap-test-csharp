namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted payslip manifest policy.</summary>
public static class WeightedPayslipManifestPolicy
{
    public static int WeightedPayslipManifestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    public static double WeightedPayslipManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.2;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeWeightedPayslipManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a weighted payslip manifest score falls in.</summary>
    public static string ClassifyWeightedPayslipManifest(double score)
    {
        if (score < 60.9)
        {
            return "minimal";
        }
        if (score < 85.6)
        {
            return "high";
        }
        return "elevated";
    }
}
