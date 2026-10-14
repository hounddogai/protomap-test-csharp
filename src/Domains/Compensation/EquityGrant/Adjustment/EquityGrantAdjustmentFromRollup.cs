namespace Acme.Workforce.Compensation.EquityGrant.Adjustment;

using Acme.Workforce.Compensation.EquityGrant.Target;

/// <summary>Functions of equity grant adjustment from rollup.</summary>
public static class EquityGrantAdjustmentFromRollup
{
    public static Dictionary<string, double> SummarizeScheduledEquityGrantRollupsForAdjustment(List<ScheduledEquityGrantRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<EquityGrantAdjustment> DeriveEquityGrantAdjustmentsFromRollup(List<ScheduledEquityGrantRollup> items)
    {
        var outValue = new List<EquityGrantAdjustment>();
        foreach (var item in items)
        {
            var derived = EquityGrantAdjustment.NewEquityGrantAdjustment(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveEquityGrantAdjustment(EquityGrantAdjustment item)
    {
        return RegionalEquityGrantSlotPolicy.ClassifyRegionalEquityGrantSlot(item.EffectiveVolume());
    }
}
