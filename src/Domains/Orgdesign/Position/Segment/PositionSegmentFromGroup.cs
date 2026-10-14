namespace Acme.Workforce.Orgdesign.Position.Segment;

using Acme.Workforce.Orgdesign.CostModel.ModelEntry;

/// <summary>Functions of position segment from group.</summary>
public static class PositionSegmentFromGroup
{
    public static Dictionary<string, double> SummarizeGlobalCostModelGroupsForSegment(List<GlobalCostModelGroup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<PositionSegment> DerivePositionSegmentsFromGroup(List<GlobalCostModelGroup> items)
    {
        var outValue = new List<PositionSegment>();
        foreach (var item in items)
        {
            var derived = PositionSegment.NewPositionSegment(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettlePositionSegment(PositionSegment item)
    {
        return TieredCostModelEntryPolicy.TieredCostModelEntryWeight(3, true);
    }
}
