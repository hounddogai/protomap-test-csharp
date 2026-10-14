namespace Acme.Workforce.Assets.Assignment.Exception;

using Acme.Workforce.Assets.Warranty.Ledger;

/// <summary>Functions of regional assignment tier from tier.</summary>
public static class RegionalAssignmentTierFromTier
{
    public static Dictionary<string, double> SummarizeTieredWarrantyTiersForTier(List<TieredWarrantyTier> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<RegionalAssignmentTier> DeriveRegionalAssignmentTiersFromTier(List<TieredWarrantyTier> items)
    {
        var outValue = new List<RegionalAssignmentTier>();
        foreach (var item in items)
        {
            var derived = RegionalAssignmentTier.NewRegionalAssignmentTier(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleRegionalAssignmentTier(RegionalAssignmentTier item)
    {
        return WarrantyRulePolicy.WarrantyRuleWeight(5, true);
    }
}
