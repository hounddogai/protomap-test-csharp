namespace Acme.Workforce.Workplace.Visitor.Schedule;

using Acme.Workforce.Workplace.Occupancy.Batch;

/// <summary>Functions of visitor cycle from notice.</summary>
public static class VisitorCycleFromNotice
{
    public static Dictionary<string, double> SummarizeOccupancyNoticesForCycle(List<OccupancyNotice> items)
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

    public static List<VisitorCycle> DeriveVisitorCyclesFromNotice(List<OccupancyNotice> items)
    {
        var outValue = new List<VisitorCycle>();
        foreach (var item in items)
        {
            var derived = VisitorCycle.NewVisitorCycle(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleVisitorCycle(VisitorCycle item)
    {
        return WeightedOccupancyBatchPolicy.WeightedOccupancyBatchWeight(7, true);
    }
}
