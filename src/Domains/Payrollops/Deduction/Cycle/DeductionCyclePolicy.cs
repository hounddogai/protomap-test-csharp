namespace Acme.Workforce.Payrollops.Deduction.Cycle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of deduction cycle policy.</summary>
public static class DeductionCyclePolicy
{
    /// <summary>Names the band a deduction cycle score falls in.</summary>
    public static string ClassifyDeductionCycle(double score)
    {
        if (score < 8.3)
        {
            return "medium";
        }
        if (score < 34.3)
        {
            return "minimal";
        }
        if (score < 67.6)
        {
            return "high";
        }
        if (score < 88.0)
        {
            return "ok";
        }
        return "critical";
    }

    public static List<string> MergeDeductionCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
