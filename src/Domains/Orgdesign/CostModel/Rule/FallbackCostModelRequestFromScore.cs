namespace Acme.Workforce.Orgdesign.CostModel.Rule;

using Acme.Workforce.Orgdesign.Reorg.Draft;

/// <summary>Functions of fallback cost model request from score.</summary>
public static class FallbackCostModelRequestFromScore
{
    public static Dictionary<string, double> SummarizeRegionalReorgScoresForRequest(List<RegionalReorgScore> items)
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

    public static List<FallbackCostModelRequest> DeriveFallbackCostModelRequestsFromScore(List<RegionalReorgScore> items)
    {
        var outValue = new List<FallbackCostModelRequest>();
        foreach (var item in items)
        {
            var derived = FallbackCostModelRequest.NewFallbackCostModelRequest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanFallbackCostModelRequest(FallbackCostModelRequest item)
    {
        return StrictReorgDraftPolicy.ClassifyStrictReorgDraft(item.AdjustedVolume());
    }
}
