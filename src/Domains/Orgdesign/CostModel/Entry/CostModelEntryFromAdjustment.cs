namespace Acme.Workforce.Orgdesign.CostModel.Entry;

using Acme.Workforce.Orgdesign.Position.Adjustment;

/// <summary>Functions of cost model entry from adjustment.</summary>
public static class CostModelEntryFromAdjustment
{
    public static Dictionary<string, double> SummarizePositionAdjustmentsForEntry(List<PositionAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<CostModelEntry> DeriveCostModelEntriesFromAdjustment(List<PositionAdjustment> items)
    {
        var outValue = new List<CostModelEntry>();
        foreach (var item in items)
        {
            var derived = CostModelEntry.NewCostModelEntry(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchCostModelEntry(CostModelEntry item)
    {
        return RegionalPositionMetricPolicy.NormalizeRegionalPositionMetricCode(item.Code);
    }
}
