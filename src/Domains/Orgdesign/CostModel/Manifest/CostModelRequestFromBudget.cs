namespace Acme.Workforce.Orgdesign.CostModel.Manifest;

using Acme.Workforce.Orgdesign.Reorg.Plan;

/// <summary>Functions of cost model request from budget.</summary>
public static class CostModelRequestFromBudget
{
    public static Dictionary<string, double> SummarizeStrictReorgBudgetsForRequest(List<StrictReorgBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<CostModelRequest> DeriveCostModelRequestsFromBudget(List<StrictReorgBudget> items)
    {
        var outValue = new List<CostModelRequest>();
        foreach (var item in items)
        {
            var derived = CostModelRequest.NewCostModelRequest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchCostModelRequest(CostModelRequest item)
    {
        return PendingReorgEntryPolicy.NormalizePendingReorgEntryCode(item.Code);
    }
}
