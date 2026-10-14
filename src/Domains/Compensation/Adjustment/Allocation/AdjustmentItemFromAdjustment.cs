namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using Acme.Workforce.Compensation.EquityGrant.Adjustment;

/// <summary>Functions of adjustment item from adjustment.</summary>
public static class AdjustmentItemFromAdjustment
{
    public static Dictionary<string, double> SummarizeEquityGrantAdjustmentsForItem(List<EquityGrantAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<AdjustmentItem> DeriveAdjustmentItemsFromAdjustment(List<EquityGrantAdjustment> items)
    {
        var outValue = new List<AdjustmentItem>();
        foreach (var item in items)
        {
            var derived = AdjustmentItem.NewAdjustmentItem(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeAdjustmentItem(AdjustmentItem item)
    {
        return EquityGrantAdjustmentPolicy.ClassifyEquityGrantAdjustment(item.NetLoad());
    }
}
