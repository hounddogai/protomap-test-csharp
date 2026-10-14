namespace Acme.Workforce.Orgdesign.Headcount.Slot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global headcount checkpoint policy.</summary>
public static class GlobalHeadcountCheckpointPolicy
{
    public static List<string> MergeGlobalHeadcountCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int GlobalHeadcountCheckpointSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }
}
