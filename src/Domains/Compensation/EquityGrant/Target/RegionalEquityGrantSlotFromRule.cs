namespace Acme.Workforce.Compensation.EquityGrant.Target;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

/// <summary>Functions of regional equity grant slot from rule.</summary>
public static class RegionalEquityGrantSlotFromRule
{
    public static Dictionary<string, double> SummarizeGlobalAdjustmentRulesForSlot(List<GlobalAdjustmentRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<RegionalEquityGrantSlot> DeriveRegionalEquityGrantSlotsFromRule(List<GlobalAdjustmentRule> items)
    {
        var outValue = new List<RegionalEquityGrantSlot>();
        foreach (var item in items)
        {
            var derived = RegionalEquityGrantSlot.NewRegionalEquityGrantSlot(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveRegionalEquityGrantSlot(RegionalEquityGrantSlot item)
    {
        return AdjustmentManifestPolicy.NormalizeAdjustmentManifestCode(item.Code);
    }
}
