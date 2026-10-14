namespace Acme.Workforce.Orgdesign.CostModel.ModelManifest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted cost model manifest policy.</summary>
public static class WeightedCostModelManifestPolicy
{
    public static int WeightedCostModelManifestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    public static List<string> MergeWeightedCostModelManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
