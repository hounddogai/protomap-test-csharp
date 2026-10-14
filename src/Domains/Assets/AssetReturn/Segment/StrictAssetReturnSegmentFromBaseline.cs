namespace Acme.Workforce.Assets.AssetReturn.Segment;

using Acme.Workforce.Assets.Assignment.Baseline;

/// <summary>Functions of strict asset return segment from baseline.</summary>
public static class StrictAssetReturnSegmentFromBaseline
{
    public static Dictionary<string, double> SummarizeLegacyAssignmentBaselinesForSegment(List<LegacyAssignmentBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<StrictAssetReturnSegment> DeriveStrictAssetReturnSegmentsFromBaseline(List<LegacyAssignmentBaseline> items)
    {
        var outValue = new List<StrictAssetReturnSegment>();
        foreach (var item in items)
        {
            var derived = StrictAssetReturnSegment.NewStrictAssetReturnSegment(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocateStrictAssetReturnSegment(StrictAssetReturnSegment item)
    {
        return StrictAssignmentBaselinePolicy.StrictAssignmentBaselineWeight(7, true);
    }
}
