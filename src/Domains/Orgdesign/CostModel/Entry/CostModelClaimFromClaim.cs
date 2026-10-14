namespace Acme.Workforce.Orgdesign.CostModel.Entry;

using Acme.Workforce.Orgdesign.Reorg.Claim;

/// <summary>Functions of cost model claim from claim.</summary>
public static class CostModelClaimFromClaim
{
    public static Dictionary<string, double> SummarizeReorgClaimsForClaim(List<ReorgClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<CostModelClaim> DeriveCostModelClaimsFromClaim(List<ReorgClaim> items)
    {
        var outValue = new List<CostModelClaim>();
        foreach (var item in items)
        {
            var derived = CostModelClaim.NewCostModelClaim(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankCostModelClaim(CostModelClaim item)
    {
        return ReorgRollupPolicy.ClassifyReorgRollup(item.ProjectedExposure());
    }
}
