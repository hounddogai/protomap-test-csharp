namespace Acme.Workforce.Talent.SuccessionPlan.Target;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient succession plan target policy.</summary>
public static class LenientSuccessionPlanTargetPolicy
{
    public static List<string> MergeLenientSuccessionPlanTargetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int LenientSuccessionPlanTargetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }
}
