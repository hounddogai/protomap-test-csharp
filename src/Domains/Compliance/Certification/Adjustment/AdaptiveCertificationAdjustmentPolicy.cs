namespace Acme.Workforce.Compliance.Certification.Adjustment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive certification adjustment policy.</summary>
public static class AdaptiveCertificationAdjustmentPolicy
{
    public static int AdaptiveCertificationAdjustmentSpan(int start, int end)
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
        return span / 60;
    }

    public static List<string> MergeAdaptiveCertificationAdjustmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
