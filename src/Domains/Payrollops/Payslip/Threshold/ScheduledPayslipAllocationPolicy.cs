namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled payslip allocation policy.</summary>
public static class ScheduledPayslipAllocationPolicy
{
    public static List<string> MergeScheduledPayslipAllocationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a scheduled payslip allocation score falls in.</summary>
    public static string ClassifyScheduledPayslipAllocation(double score)
    {
        if (score < 16.9)
        {
            return "medium";
        }
        if (score < 27.0)
        {
            return "high";
        }
        if (score < 35.7)
        {
            return "watch";
        }
        if (score < 60.6)
        {
            return "elevated";
        }
        return "minimal";
    }
}
