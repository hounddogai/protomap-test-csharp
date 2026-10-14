namespace Acme.Workforce.Payrollops.Deduction.Quota;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient deduction entry policy.</summary>
public static class LenientDeductionEntryPolicy
{
    /// <summary>Names the band a lenient deduction entry score falls in.</summary>
    public static string ClassifyLenientDeductionEntry(double score)
    {
        if (score < 26.2)
        {
            return "elevated";
        }
        if (score < 40.5)
        {
            return "minimal";
        }
        return "low";
    }

    public static List<string> MergeLenientDeductionEntryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double LenientDeductionEntryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.66;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
