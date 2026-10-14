namespace Acme.Workforce.Compensation.BonusPool.Claim;

using Acme.Workforce.Compensation.EquityGrant.GrantLedger;

/// <summary>Functions of weighted bonus pool quota from draft.</summary>
public static class WeightedBonusPoolQuotaFromDraft
{
    public static Dictionary<string, double> SummarizeGlobalEquityGrantDraftsForQuota(List<GlobalEquityGrantDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<WeightedBonusPoolQuota> DeriveWeightedBonusPoolQuotasFromDraft(List<GlobalEquityGrantDraft> items)
    {
        var outValue = new List<WeightedBonusPoolQuota>();
        foreach (var item in items)
        {
            var derived = WeightedBonusPoolQuota.NewWeightedBonusPoolQuota(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double NormalizeWeightedBonusPoolQuota(WeightedBonusPoolQuota item)
    {
        return GlobalEquityGrantDraftPolicy.GlobalEquityGrantDraftWeight(4, true);
    }
}
