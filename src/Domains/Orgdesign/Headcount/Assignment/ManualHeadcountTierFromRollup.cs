namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

using Acme.Workforce.Orgdesign.Reorg.Claim;

/// <summary>Functions of manual headcount tier from rollup.</summary>
public static class ManualHeadcountTierFromRollup
{
    public static Dictionary<string, double> SummarizeReorgRollupsForTier(List<ReorgRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<ManualHeadcountTier> DeriveManualHeadcountTiersFromRollup(List<ReorgRollup> items)
    {
        var outValue = new List<ManualHeadcountTier>();
        foreach (var item in items)
        {
            var derived = ManualHeadcountTier.NewManualHeadcountTier(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectManualHeadcountTier(ManualHeadcountTier item)
    {
        return ReorgClaimPolicy.ClassifyReorgClaim(item.EffectiveExposure());
    }
}
