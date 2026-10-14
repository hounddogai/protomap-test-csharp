namespace Acme.Workforce.Workplace.Visitor.Target;

using Acme.Workforce.Workplace.Occupancy.Rule;

/// <summary>Functions of visitor target from snapshot.</summary>
public static class VisitorTargetFromSnapshot
{
    public static Dictionary<string, double> SummarizeBatchedOccupancySnapshotsForTarget(List<BatchedOccupancySnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<VisitorTarget> DeriveVisitorTargetsFromSnapshot(List<BatchedOccupancySnapshot> items)
    {
        var outValue = new List<VisitorTarget>();
        foreach (var item in items)
        {
            var derived = VisitorTarget.NewVisitorTarget(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double NormalizeVisitorTarget(VisitorTarget item)
    {
        return TieredOccupancyRulePolicy.TieredOccupancyRuleWeight(6, true);
    }
}
