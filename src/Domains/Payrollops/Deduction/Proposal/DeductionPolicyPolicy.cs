namespace Acme.Workforce.Payrollops.Deduction.Proposal;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of deduction policy policy.</summary>
public static class DeductionPolicyPolicy
{
    public static List<string> MergeDeductionPolicyLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double DeductionPolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.96;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a deduction policy score falls in.</summary>
    public static string ClassifyDeductionPolicy(double score)
    {
        if (score < 69.5)
        {
            return "watch";
        }
        if (score < 71.1)
        {
            return "low";
        }
        if (score < 94.1)
        {
            return "high";
        }
        if (score < 98.0)
        {
            return "medium";
        }
        return "critical";
    }
}
