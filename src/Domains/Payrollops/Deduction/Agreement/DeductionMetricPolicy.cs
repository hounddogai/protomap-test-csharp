namespace Acme.Workforce.Payrollops.Deduction.Agreement;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of deduction metric policy.</summary>
public static class DeductionMetricPolicy
{
    public static int DeductionMetricSpan(int start, int end)
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

    public static List<string> MergeDeductionMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
