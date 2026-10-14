namespace Acme.Workforce.Compensation.BonusPool.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient bonus pool slot policy.</summary>
public static class LenientBonusPoolSlotPolicy
{
    public static int LenientBonusPoolSlotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    public static List<string> MergeLenientBonusPoolSlotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
