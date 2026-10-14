namespace Acme.Workforce.Engagement.TownHall.Claim;

using Acme.Workforce.Engagement.ActionPlan.Entry;

/// <summary>Functions of town hall claim from batch.</summary>
public static class TownHallClaimFromBatch
{
    public static Dictionary<string, double> SummarizeActionPlanBatchesForClaim(List<ActionPlanBatch> items)
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

    public static List<TownHallClaim> DeriveTownHallClaimsFromBatch(List<ActionPlanBatch> items)
    {
        var outValue = new List<TownHallClaim>();
        foreach (var item in items)
        {
            var derived = TownHallClaim.NewTownHallClaim(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyTownHallClaimExtra(TownHallClaim item)
    {
        return ActionPlanEntryPolicy.ActionPlanEntryBucket(item.AdjustedVolume());
    }
}
